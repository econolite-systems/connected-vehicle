// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    //Note:  the documentation and what we are actually seeing aren't the same.  I think it is because the "count" stuff doesn't work on a timeseries.
    //For some reason the MongoDB Driver library doesn't have the collection.GetStats() and a CollectionStatsResult so manually implementing portions of it.  
    //Only including the stat properties that we are using.  The contents of the storageStats varies  based on the storage engine in use.
    public class CollectionStatsResult
    {
        /// <summary>
        /// The total number of documents in the collection; can be inaccurate if there are shards
        /// This is set if count is added to the collStats aggregate
        /// </summary>
        //public long Count { get; set; } = 0;

        /// <summary>
        /// The storage stats for the collection
        /// </summary>
        public StorageStatsResult StorageStats { get; set; } = new StorageStatsResult();
    }

    public class StorageStatsResult
    {
        //Note:  the documentation says this exists but it doesn't seem to be in the results; this would be per shard;
        //I think its not available on a timeseries collection.
        /// <summary>
        /// The total number of documents in the collection
        /// </summary>
        //public long Count { get; set; } = 0;

        /// <summary>
        /// The total size (storageSize + totalIndexSize) of the collection in bytes.  
        /// </summary>
        public long TotalSize { get; set; } = 0;
    }
}
