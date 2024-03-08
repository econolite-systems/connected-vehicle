// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Azure.Storage.Blobs.Models;
using Econolite.Ode.Cloud.Common.Models;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public interface IConnectedVehicleArchiveLogRepository

    {
        /// <summary>
        /// Inserts a Connected Vehicle Message into the archive repository
        /// </summary>
        /// <param name="message">The connected vehicle status message</param>
        /// <returns></returns>
        Task InsertAsync(ConnectedVehicleMessageDocument message);

        /// <summary>
        /// Gets the Azure container summary stats including the total number and size of blobs.  Probably costs $ every time we call so use cautiously.
        /// </summary>
        /// <param name="includeBlobs">Optional param.  If true it will return the container's list of blogs.</param>
        /// <returns></returns>
        Task<AzureContainerSummary> GetContainerSummaryAsync(bool includeBlobs = false);

        /// <summary>
        /// Calculates the total count and size for the archive repository type.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryType();

        /// <summary>
        /// Searches the Azure blob storage by tags
        /// </summary>
        /// <param name="query">The query to be search through the tags</param>
        /// <returns></returns>
        Task<IEnumerable<TaggedBlobItem>> FindBlobsByTagsAsync(string query);

        /// <summary>
        /// Deletes an Azure blob storage
        /// </summary>
        /// <param name="blobName">The name of the blob to delete</param>
        /// <returns></returns>
        Task DeleteBlobIfExistsAsync(string blobName);

        /// <summary>
        /// Deletes the oldest records from an Azure blob storage if it exceeds a max size
        /// </summary>
        /// <param name="configArchiveMaxSize">The user configurable maximum size of the archive repository</param>
        /// <returns></returns>
        Task DeleteArchiveBySize(long configArchiveMaxSize);

        /// <summary>
        /// Deletes an Azure blob storage by the timeStamp tag
        /// </summary>
        /// <param name="endTimeStamp">The time stamp to search for. Deletes all records >= the endTimeStamp</param>
        /// <returns></returns>
        Task DeleteArchiveByDate(DateTime endTimeStamp);

    }
}
