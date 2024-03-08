// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Cloud.Common.Managers;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Econolite.Ode.Persistence.Mongo.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Azure.Storage.Blobs.Models;
using System.Text;
using Econolite.Ode.Cloud.Common.Models;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.ConnectedVehicle.Messaging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Repository.ConnectedVehicle.Messaging;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public class ConnectedVehicleArchiveLogRepository : IConnectedVehicleArchiveLogRepository
    {
        ILogger<ConnectedVehicleArchiveLogRepository> _logger;
        private readonly IMongoCollection<ConnectedVehicleAzureTrackingDocument> _cvAzureBlobTrackingCollection;
        private readonly IMongoCollection<ConnectedVehicleAzureDailyTotalsDocument> _cvAzureDailyTotalsCollection;
        private readonly string _connectionStr = String.Empty;
        private readonly string _containerName = String.Empty;
        private readonly bool _enabled = true;

        public ConnectedVehicleArchiveLogRepository(IConfiguration configuration, IMongoContext mongoContext, ILogger<ConnectedVehicleArchiveLogRepository> logger)
        {
            _logger = logger;
            //this collection keeps a record of ever blob's timestamp and size that we write to azure; querying this recordset will be faster and cheaper than querying azure
            _cvAzureBlobTrackingCollection = mongoContext.GetCollection<ConnectedVehicleAzureTrackingDocument>(configuration["Collections:ConnectedVehicleAzureBlobTracking"] ?? throw new NullReferenceException("Collections:ConnectedVehicleAzureBlobTracking missing in configuration"));
            _cvAzureDailyTotalsCollection = mongoContext.GetCollection<ConnectedVehicleAzureDailyTotalsDocument>(configuration["Collections:ConnectedVehicleAzureDailyTotals"] ?? throw new NullReferenceException("Collections:ConnectedVehicleAzureDailyTotals missing in configuration"));
            _connectionStr = configuration["ConnectionStrings:AzureBlobCoolStorageConnectionString"] ?? throw new NullReferenceException("ConnectionStrings:AzureBlobCoolStorageConnectionString missing in configuration");
            _containerName = configuration["Containers:ConnectedVehicleLog"] ?? throw new NullReferenceException("Containers:ConnectedVehicleLog missing in configuration");
            _enabled = !string.IsNullOrEmpty(_connectionStr);
        }

        #region add to archive

        public async Task InsertAsync(ConnectedVehicleMessageDocument message)
        {
            //save to Azure blob
            var isSuccessful = _enabled && await InsertAzureBlobAsync(message);

            if (isSuccessful)
            {
                //save to Mongo tracking
                await InsertMongoAzureTrackingAsync(message);
            }
        }

        private async Task<bool> InsertAzureBlobAsync(ConnectedVehicleMessageDocument message)
        {
            try
            {
                //build tag options for the blob for easier searching
                var timeStamp = message.TimeStamp;
                //set the Azure tags for easier searching
                var options = new BlobUploadOptions();
                options.Tags = new Dictionary<string, string>
                {
                    { "TimeStamp", $"{timeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}" },
                };

                //convert the json to a byte array; make sure to use our same serializer
                var jsonString = System.Text.Json.JsonSerializer.Serialize(message, JsonPayloadSerializerOptions.Options);

                byte[] data = Encoding.ASCII.GetBytes(jsonString);

                //set the folder structure and file name to store the json file.
                var id = Guid.NewGuid();
                var filename = $"{id:D}_{timeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}.json";
                var blobName = $"{timeStamp.Year}/{timeStamp.Month.ToString().PadLeft(2, '0')}/{timeStamp.Day.ToString().PadLeft(2, '0')}/{filename}";

                //Add to the Azure blob storage
                await AzureStorageManager.UploadDataAsync(_connectionStr, _containerName, blobName, data, options);

                //return a success/failure bit
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to add message to Azure blob storage: {message.ToString()}");
                return false;
            }
        }

        private async Task InsertMongoAzureTrackingAsync(ConnectedVehicleMessageDocument message)
        {
            try
            {
                //add to the Mongo Azure tracking table
                await _cvAzureBlobTrackingCollection.InsertOneAsync(message.AdaptToAzureTracking());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to add message to Azure Tracking: {message.ToString()}");
            }
        }

        #endregion

        #region queries

        public async Task<AzureContainerSummary> GetContainerSummaryAsync(bool includeBlobs = false)
        {
            //Note:  concerned about the performance on this; and probably costs $ every time we call it
            return await AzureStorageManager.GetContainerSummaryAsync(_connectionStr, _containerName, includeBlobs);
        }

        public async Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryType()
        {
            //sum our daily calculations in Mongo to get a grand total
            var query = _cvAzureDailyTotalsCollection.Aggregate()
                 .Group(new BsonDocument {
                     { "_id", ConnectedVehicleRepositoryTypeEnum.Archive },
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

        public async Task<IEnumerable<TaggedBlobItem>> FindBlobsByTagsAsync(string query)
        {
            //NOTE:  each call costs $ so don't be frivolous
            var blobs = await AzureStorageManager.FindBlobsByTagsAsync(_connectionStr, _containerName, query);
            return blobs;
        }

        #endregion

        #region delete

        public async Task DeleteBlobIfExistsAsync(string blobName)
        {
            await AzureStorageManager.DeleteBlobIfExistsAsync(_connectionStr, _containerName, blobName);
        }

        public async Task DeleteArchiveBySize(long configArchiveMaxSize)
        {
            //deletes one day of data at a time until we get below the max storage amount for the repository.  utilizes a Mongo view that calculates each day's total size.
            try
            {
                //get the azure totals
                long azureByteSize = 0;
                var summary = await GetTotalsByRepositoryType();
                if (summary.Any())
                {
                    azureByteSize = summary.First().ByteSize;
                }
                _logger.LogDebug($"DeleteArchiveBySize: archiveSize={azureByteSize}, configArchiveMaxSize={configArchiveMaxSize}");

                //is the repo size greater than the config max settings
                if (azureByteSize > configArchiveMaxSize)
                {
                    //calculate how many bytes we need to delete
                    var bytesToDelete = ConnectedVehicleRepositoryHelper.GetBytesToPurge(azureByteSize, configArchiveMaxSize);
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

                        //delete from azure; this could take a while
                        await DeleteArchiveByDate(endTimeStamp);
                    } 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to delete data by oldest: {configArchiveMaxSize}.");
            }
        }

        private async Task<ConnectedVehicleAzureDailyTotalsDocument?> GetNextOldestDay(DateTime startDate)
        {
            //the data in the view is 00:00:00
            var gteLastDate = Builders<ConnectedVehicleAzureDailyTotalsDocument>.Filter.Gt(s => s.dayOfYear, startDate);

            //Get the next oldest day that's stored in the archive
            var query = _cvAzureDailyTotalsCollection.Aggregate()
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

        public async Task DeleteArchiveByDate(DateTime endTimeStamp)
        {
            try
            {
                //Format needs to be like this for lexicographic sorting
                //2022-01-01T00:00:00.000Z
                var timestamp = $"{endTimeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}";

                //azure query format wants double quotes around the key and single quotes around the values
                var query = String.Format(@"""TimeStamp"" <= '{0}'", timestamp);
                _logger.LogDebug("Find blob by tag query: " + query);

                var blobs = await FindBlobsByTagsAsync(query);
                _logger.LogDebug("Found blob count: " + blobs.Count());

                //delete from Azure blob
                deleteBlobs(blobs);

                //delete Mongo's Azure tracking data
                deleteArchiveTrackingByDate(endTimeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to delete data by date: {endTimeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}.");
            }
        }

        private async void deleteArchiveTrackingByDate(DateTime endTimeStamp)
        {
            try
            {
                //delete the mongo tracking records; this could take a while
                var beforeEnd = Builders<ConnectedVehicleAzureTrackingDocument>.Filter.Lte(s => s.MetaData.TimeStamp, endTimeStamp);
                var results = await _cvAzureBlobTrackingCollection.DeleteManyAsync(beforeEnd);
                var count = results.DeletedCount;
                _logger.LogDebug($"Deleted {count} documents from Mongo azure tracking collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to delete archive tracking data by date: {endTimeStamp:yyyy-MM-dd'T'HH:mm:ss.fffK}.");
            }
        }

        private async void deleteBlobs(IEnumerable<TaggedBlobItem> blobs)
        {
            foreach (var filteredBlob in blobs)
            {
                var tags = String.Join(", ", filteredBlob.Tags.Select(res => "Key " + res.Key + ": ID = " + res.Value));
                _logger.LogDebug($"BlobIndex result: ContainerName= {filteredBlob.BlobContainerName}, " +
                    $"BlobName= {filteredBlob.BlobName}" + $"BlobTags= {tags}");
                await DeleteBlobIfExistsAsync(filteredBlob.BlobName);
            }
        }

        #endregion
    }
}
