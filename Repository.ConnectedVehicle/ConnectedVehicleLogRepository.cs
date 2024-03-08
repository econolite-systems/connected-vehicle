// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Econolite.Ode.Persistence.Mongo.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public class ConnectedVehicleLogRepository : IConnectedVehicleLogRepository
    {
        private readonly ILogger<ConnectedVehicleLogRepository> _logger;
        private readonly IMongoCollection<ConnectedVehicleMessageDocument> _cvLogCollection;
        private readonly IMongoCollection<ConnectedVehicleMessageTypeMinuteTotalsDocument> _cvMinuteTotals;
        private readonly IMongoCollection<ConnectedVehicleIntersectionTotalsDocument> _cvIntersectionTotals;
        private readonly IMongoCollection<ConnectedVehicleMessageTypeTotalsDocument> _cvMessageTypeTotals;
        private readonly IMongoCollection<ConnectedVehicleLogDailyTotalsDocument> _cvLogDailyTotalsCollection;

        public ConnectedVehicleLogRepository(IConfiguration configuration, IMongoContext mongoContext, ILogger<ConnectedVehicleLogRepository> logger)
        {
            _cvLogCollection = mongoContext.GetCollection<ConnectedVehicleMessageDocument>(configuration["Collections:ConnectedVehicleLog"] ?? throw new NullReferenceException("Collections:ConnectedVehicleLog missing in configuration"));
            _cvMinuteTotals = mongoContext.GetCollection<ConnectedVehicleMessageTypeMinuteTotalsDocument>(configuration["Collections:ConnectedVehicleMessageTypeMinuteTotals"] ?? throw new NullReferenceException("Collections:ConnectedVehicleMessageTypeMinuteTotals missing in configuration"));
            _cvIntersectionTotals = mongoContext.GetCollection<ConnectedVehicleIntersectionTotalsDocument>(configuration["Collections:ConnectedVehicleIntersectionTotals"] ?? throw new NullReferenceException("Collections:ConnectedVehicleIntersectionTotals missing in configuration"));
            _cvMessageTypeTotals = mongoContext.GetCollection<ConnectedVehicleMessageTypeTotalsDocument>(configuration["Collections:ConnectedVehicleMessageTypeTotals"] ?? throw new NullReferenceException("Collections:ConnectedVehicleMessageTypeTotals missing in configuration"));
            _cvLogDailyTotalsCollection = mongoContext.GetCollection<ConnectedVehicleLogDailyTotalsDocument>(configuration["Collections:ConnectedVehicleLogDailyTotals"] ?? throw new NullReferenceException("Collections:ConnectedVehicleLogDailyTotals missing in configuration"));
            _logger = logger;
        }

        #region ConnectedVehicleLog

        public async Task InsertAsync(ConnectedVehicleMessageDocument statusMessage)
        {
            await _cvLogCollection.InsertOneAsync(statusMessage);
        }

        public async Task<List<ConnectedVehicleMessageDocument>> Find(DateTime startDate, DateTime? endDate)
        {
            return await _queryDatesAsync(startDate, endDate);
        }

        public async Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryType()
        {
            //Note: Tried with GetCollectionStats() but numbers seemed off and feel like we need to use the same numbers consistently
            //var result = await GetCollectionStats();

            //Note:  the Mongo records will be larger than our calculated byte size due to indexes.

            //sum our daily calculations in Mongo to get a grand total
            var query = _cvLogDailyTotalsCollection.Aggregate()
                 .Group(new BsonDocument {
                     { "_id", ConnectedVehicleRepositoryTypeEnum.Working },
                     { "messageCount", new BsonDocument("$sum", "$messageCount") },
                     { "byteSize", new BsonDocument("$sum", "$byteSize") }
                 })
                 //change the _id field to the type field
                 .Project(new BsonDocument {
                     { "type", "$_id" },
                     { "messageCount", "$messageCount" },
                     { "byteSize", "$byteSize" }
                 });
            var results = await query.ToListAsync();

            var totals = results.Select(r => BsonSerializer.Deserialize<ConnectedVehicleRepositoryTypeCountAndSize>(r)!);

            return totals;
        }

        public async Task<long> GetTotalMessageCount()
        {
            return await GetDocumentCount();
        }

        #endregion

        #region ConnectedVehicleMessageTypeMinuteTotals On-Demand Materialized View

        public async Task<IEnumerable<ConnectedVehicleMessageTypeCount>> GetLastHourTotalsByMessageType()
        {
            var lastHour = DateTime.Now.AddMinutes(-60);
            var gteLastHour = Builders<ConnectedVehicleMessageTypeMinuteTotalsDocument>.Filter.Gte(s => s.minuteOfDay, lastHour);

            var query = _cvMinuteTotals.Aggregate()
                 //get the last 60 mins worth of data
                 .Match(gteLastHour)
                 //group on a constant field so we get all documents and then count the messages
                 .Group(new BsonDocument {
                     { "_id", "$type" },
                     { "messageCount", new BsonDocument("$sum", "$messageCount") }
                 })
                 //change the _id field to the type field
                 .Project(new BsonDocument {
                     { "type", "$_id" },
                     { "messageCount", "$messageCount" }
                 })
                .Sort(new BsonDocument { { "type", 1 } });
            var results = await query.ToListAsync();

            //convert the bsonDoc to our model
            var totals = results.Select(r => BsonSerializer.Deserialize<ConnectedVehicleMessageTypeCount>(r)!);

            return totals;
        }

        public async Task<DateTime?> FindConnectedVehicleMessageTypeMinuteTotalsLastModified()
        {
            //Get the latest last modified date
            var query = _cvMinuteTotals.Aggregate()
                .Sort(new BsonDocument { { "lastModified", -1 } })
                 //take the 1st record
                 .Limit(1);

            var results = await query.ToListAsync();

            DateTime? lastModified = null;
            if (results.Any())
            {
                lastModified = results.First().lastModified;
            }
            return lastModified;
        }

        public async Task<DateTime> UpdateConnectedVehicleMessageTypeMinuteTotals()
        {
            DateTime? lastModifiedDate = await FindConnectedVehicleMessageTypeMinuteTotalsLastModified();

            //if there isn't a last modified date then either this is the 1st run or the TTL index on the view cleared out the > 65 mins of data and we're starting fresh.
            //for safety sakes go back 5 mins since we don't know how long its been idle
            var startDate = lastModifiedDate ?? DateTime.Now.AddMinutes(-5);

            //Note: this is the exported aggregate script from MongoDB Compass;
            //create-connected-vehicle-log.mongodb has the node export if you want to pull that into Compass and play with it
            var pipeline = new[] {
                new BsonDocument("$match", //get all the records from ConnectedVehicleLog since the last run
                new BsonDocument("timeStamp",
                new BsonDocument("$gt",    startDate))), //filter for dates greater than the last time we ran the script and updated the data
                new BsonDocument("$group",//group on the minute of the day and the type
                new BsonDocument
                    {
                        { "_id",
                new BsonDocument
                        {
                            { "minuteOfDay",
                new BsonDocument("$dateFromParts",
                new BsonDocument
                                {
                                    { "year",
                new BsonDocument("$year", "$timeStamp") },
                                    { "month",
                new BsonDocument("$month", "$timeStamp") },
                                    { "day",
                new BsonDocument("$dayOfMonth", "$timeStamp") },
                                    { "hour",
                new BsonDocument("$hour", "$timeStamp") },
                                    { "minute",
                new BsonDocument("$minute", "$timeStamp") }
                                }) },
                            { "type", "$metaData.type" }
                        } },
                        { "messageCount",//count the messages
                new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$project",//make a new data structure with just what we need.  remove the id since we'll add our own.
                new BsonDocument
                    {
                        { "_id", 0 },
                        { "minuteOfDay", "$_id.minuteOfDay" },
                        { "type", "$_id.type" },
                        { "messageCount", "$messageCount" },
                        { "lastModified", DateTime.UtcNow } //update the lastModified date with our new run time
                    }),
                new BsonDocument("$sort",
                new BsonDocument
                    {
                        { "minuteOfDay", 1 },
                        { "type", 1 }
                    }),
                new BsonDocument("$merge",//merge to ConnectedVehicleMessageTypeMinuteTotals on-demand materialized view 
                new BsonDocument
                    {
                        { "into", "ConnectedVehicleMessageTypeMinuteTotals" },
                        { "on",
                new BsonArray
                        {
                            "minuteOfDay",
                            "type"
                        } },
                        { "whenMatched", //if there's already a record in there for this group just add the new count to the existing count
                new BsonArray
                        {
                            new BsonDocument("$project",
                            new BsonDocument
                                {
                                    { "minuteOfDay", "$minuteOfDay" },
                                    { "type", "$type" },
                                    { "lastModified", "$$new.lastModified" },
                                    { "messageCount",
                            new BsonDocument("$sum",
                            new BsonArray
                                        {
                                            "$messageCount",
                                            "$$new.messageCount"
                                        }) }
                                })
                        } },
                        { "whenNotMatched", "insert" } //if there's no match on the group then insert a record
                    })
            };

            //run the aggregate pipeline; just updates data so nothing to return
            var _ = await _cvLogCollection.AggregateAsync<BsonDocument>(pipeline);
            //var results = await _.ToListAsync();

            _logger.LogDebug("Updating Connected Vehicle Minute Totals since {@}", startDate);

            return startDate;
        }

        #endregion

        #region ConnectedVehicleIntersectionTotals view

        public async Task<IEnumerable<ConnectedVehicleIntersectionTypeCountAndSize>> GetIntersectionTotals()
        {
            IEnumerable<ConnectedVehicleIntersectionTypeCountAndSize> totals = new List<ConnectedVehicleIntersectionTypeCountAndSize>();
            try
            {
                //get the totals of all time - get all the records
                var pipeline = new[] {
                    new BsonDocument("$sort",
                        new BsonDocument
                         {
                            { "intersectionId", 1 }
                        }),
                };
                var cursor = await _cvIntersectionTotals.AggregateAsync<BsonDocument>(pipeline);
                var results = await cursor.ToListAsync();

                //convert the bsonDoc to our model
                totals = results.Select(r => BsonSerializer.Deserialize<ConnectedVehicleIntersectionTypeCountAndSize>(r)!);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception thrown while calculating Intersection Totals");
            }
            return totals;
        }

        #endregion

        #region UpdateConnectedVehicleMessageTypeTotals View

        public async Task<IEnumerable<ConnectedVehicleMessageTypeCountAndSize>> GetTotalsByMessageType()
        {
            //get the totals of all time - get all the records
            var pipeline = new[] {
                    new BsonDocument("$sort",
                        new BsonDocument
                         {
                            { "type", 1 }
                        }),
                };
            var cursor = await _cvMessageTypeTotals.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            //Convert the bsonDoc to our model            
            var totals = results.Select(r => BsonSerializer.Deserialize<ConnectedVehicleMessageTypeCountAndSize>(r)!);

            return totals;
        }


        #endregion

        #region Purge ConnectedVehicleLog

        public async Task DeleteLogBySize(long configLogMaxSize)
        {
            try
            {
                //get the Mongo summary info
                long logByteSize = 0;
                var summary = await GetTotalsByRepositoryType();
                if(summary.Any())
                {
                    //Note:  the Mongo records will be larger than our calculated byte size due to indexes. Since we are deleting the entire day it might give us enough cushion to not have to worry about the indexes
                    logByteSize = summary.First().ByteSize;
                }
                _logger.LogDebug($"DeleteLogBySize: archiveSize={logByteSize}, configLogMaxSize={configLogMaxSize}");

                //is the repo size greater than the config max settings
                if (logByteSize > configLogMaxSize)
                {
                    //calculate how many bytes we need to delete
                    var bytesToDelete = ConnectedVehicleRepositoryHelper.GetBytesToPurge(logByteSize, configLogMaxSize);
                    _logger.LogDebug($"bytesToDelete={bytesToDelete}");

                    //the running total of how many bytes have actually been deleted
                    long runningTotalBytesDeleted = 0;

                    //keep track of the last date so we can get the next oldest one
                    DateTime currentDate = DateTime.MinValue;

                    //make a list of all the days we need to delete
                    var daysToDelete = new List<DateTime>();

                    //continue deleting until we have deleted enough to get us under our repository cap
                    while (bytesToDelete > 0 && bytesToDelete >= runningTotalBytesDeleted)
                    {

                        //get the next oldest date
                        var nextOldestDay = await GetNextOldestDay(currentDate);
                        if (nextOldestDay != null)
                        {
                            //store the date off so we can calculate the next one
                            currentDate = nextOldestDay.dayOfYear;
                            _logger.LogDebug($"nextOldestDay={nextOldestDay.dayOfYear}");

                            //add the date to the list to archive
                            daysToDelete.Add(currentDate);

                            //add the size to our running total and repeat until we hit the bytesToDelete
                            //Note:  the Mongo records will be larger than our calculated byte size due to indexes. Since we are deleting the entire day it might give us enough cushion to not have to worry about the indexes
                            runningTotalBytesDeleted += nextOldestDay.ByteSize;
                            _logger.LogDebug($"runningTotalBytesDeleted={runningTotalBytesDeleted}");
                        }
                        else
                        {
                            //if we don't have any data to work with turn things off
                            break;
                        }
                    }

                    if (daysToDelete.Any())
                    {
                        //get the last day and delete everything before it; list should already be in ascending order
                        DateTime endDate = daysToDelete.Last();

                        //The timestamp is at 00:00:00 and the filters look for <= so we need the end of the day
                        var endTimeStamp = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);
                        _logger.LogDebug($"Deleting: endTimeStamp<={endTimeStamp}");

                        await DeleteLogByDate(endTimeStamp);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to delete data by oldest: {configLogMaxSize}.");
            }
        }
        private async Task<ConnectedVehicleLogDailyTotalsDocument?> GetNextOldestDay(DateTime startDate)
        {
            //the data in the view is 00:00:00
            var gteLastDate = Builders<ConnectedVehicleLogDailyTotalsDocument>.Filter.Gt(s => s.dayOfYear, startDate);

            //Get the next oldest day that's stored in the archive
            var query = _cvLogDailyTotalsCollection.Aggregate()
                .Match(gteLastDate)
                .Sort(new BsonDocument { { "dayOfYear", 1 } })
                 //take the 1st record
                 .Limit(1);

            var results = await query.ToListAsync();
            if (results.Any())
            {
                var nextTotals = results.First();
                _logger.LogDebug($"Next oldest day totals: day={nextTotals.dayOfYear} size={nextTotals.ByteSize}.");
                return nextTotals;
            }
            else
            {
                _logger.LogDebug($"Next oldest day totals: not found.");
                return null;
            }
        }

        public async Task DeleteLogByDate(DateTime endTimeStamp)
        {
            var beforeEnd = Builders<ConnectedVehicleMessageDocument>.Filter.Lte(s => s.MetaData.TimeStamp, endTimeStamp);
            var results = await _cvLogCollection.DeleteManyAsync(beforeEnd);
            var count = results.DeletedCount;
            _logger.LogDebug($"Deleted {count} documents from log collection");
        }

        #endregion

        #region supporting functions

        private static FilterDefinition<ConnectedVehicleMessageDocument> _makeDateRangeFilter(DateTime startDate, DateTime? endDate)
        {
            var afterStart = Builders<ConnectedVehicleMessageDocument>.Filter.Gte(s => s.TimeStamp, startDate);

            if (endDate is null) return afterStart;

            var beforeEnd = Builders<ConnectedVehicleMessageDocument>.Filter.Lte(s => s.TimeStamp, endDate);
            return afterStart & beforeEnd;
        }

        private async Task<List<ConnectedVehicleMessageDocument>> _queryDatesAsync(DateTime startDate, DateTime? endDate)
        {
            //Note: - The Find functions were failing deserializing the object.  Error = invalid json at "N.  The json was ending up with "NumberLong(x)".
            //Fixed the issue by changing the bson to json serializer in common to use OutputMode = JsonOutputMode.CanonicalExtendedJson
            var cursor = await _cvLogCollection.FindAsync(_makeDateRangeFilter(startDate, endDate));
            return await cursor.ToListAsync();
        }

        private async Task<ConnectedVehicleRepositoryTypeCountAndSize> GetCollectionStats()
        {
            //Note:  this was giving me some strange results so abandoned its use for now
            //Note:  the documentation and what we are actually seeing aren't the same.  I think it is because the "count" stuff doesn't work on a timeseries.
            //From the docs:
            //For a collection in a replica set or a non-sharded collection in a cluster, $collStats outputs a single document.For a sharded collection, $collStats outputs one document per shard
            //.Count vs storageStats.Count = The .count is based on the collection's metadata, which provides a fast but sometimes inaccurate count for sharded clusters.  storageStats returns one document per shard.
            var pipeline = new[] {
                new BsonDocument("$collStats",
                new BsonDocument
                    {
                        { "storageStats",
                new BsonDocument("scale", 1) }, //return size data in bytes
                        //{ "count", new BsonDocument() } //Note: this gives an error  on a timeseries collection
                    })
            };
            var cursor = await _cvLogCollection.AggregateAsync<CollectionStatsResult>(pipeline);
            var results = await cursor.ToListAsync();

            //need to sum up the data in case we get multiple records back 1 per shard
            var grandTotal = new ConnectedVehicleRepositoryTypeCountAndSize();
            if (results.Any())
            {
                grandTotal.ByteSize = results.Sum(s => s.StorageStats.TotalSize);
                //grandTotal.MessageCount = results.Sum(s => s.StorageStats.Count); //Note: documentation says there's a count but i don't see one; maybe because timeseries won't give a count
            }

            var count = await GetDocumentCount();
            grandTotal.MessageCount = count;

            return grandTotal;

        }

        private async Task<long> GetDocumentCount()
        {
            //gives a fast estimate of the total documents using metadata; if this isn't accurate enough we can try CountDocumentsAsync
            return await _cvLogCollection.EstimatedDocumentCountAsync();
        }

        #endregion
    }
}
