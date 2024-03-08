// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Cloud.Common.Models;
using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public interface IConnectedVehicleLogRepository
    {
        /// <summary>
        ///     Inserts a new connected vehicle message entry into the collection.
        /// </summary>
        /// <param name="log">The new connected vehicle message to log</param>
        Task InsertAsync(ConnectedVehicleMessageDocument log);

        /// <summary>
        ///     Finds Connected Vehicle Logs with a timestamp between the
        ///     given start and end dates. If the end date is not given, returns all statuses with a timestamp
        ///     after the given start date.
        /// </summary>
        /// <param name="startDate">Mandatory start date for filtering log entries</param>
        /// <param name="endDate">Optional end date for filtering log entries</param>
        /// <returns>A list of connected vehicle message objects</returns>
        Task<List<ConnectedVehicleMessageDocument>> Find(DateTime startDate, DateTime? endDate);

        /// <summary>
        /// Calculates the total number and size of connected vehicle messages grouped by each connected vehicle message type (ConnectedVehicleMessageTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleMessageTypeCountAndSize>> GetTotalsByMessageType();

        /// <summary>
        /// Calculates the total number and size of connected vehicle messages by repository type (ConnectedVehicleRepositoryTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryType();

        /// <summary>
        /// Gets the last date and time records were updated in the ConnectedVehicleMessageTypeMinuteTotals on-demand materialized view
        /// </summary>
        /// <returns></returns>
        Task<DateTime?> FindConnectedVehicleMessageTypeMinuteTotalsLastModified();

        /// <summary>
        /// Updates the count of the messages per type per M/D/Y H:M. Queries the data for any logs greater than the lastModified date so it only get the latest changes.
        /// Takes the new counts and adds them to the existing count from the last modification.
        /// </summary>
        /// <returns></returns>
        Task<DateTime> UpdateConnectedVehicleMessageTypeMinuteTotals();

        /// <summary>
        /// Counts the last 60 mins of messages by message type (ConnectedVehicleMessageTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleMessageTypeCount>> GetLastHourTotalsByMessageType();

        /// <summary>
        /// Counts the total number of connected vehicle messages
        /// </summary>
        /// <returns></returns>
        Task<long> GetTotalMessageCount();

        /// <summary>
        /// Calculates the message totals over all time per intersection
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleIntersectionTypeCountAndSize>> GetIntersectionTotals();

        /// <summary>
        /// Deletes the oldest records CV Log messages if the repo exceeds a max size
        /// </summary>
        /// <param name="configArchiveMaxSize">The user configurable maximum size of the archive repository</param>
        /// <returns></returns>
        Task DeleteLogBySize(long configArchiveMaxSize);

        /// <summary>
        /// Deletes CV log messages by the timeStamp.
        /// </summary>
        /// <param name="endTimeStamp">The time stamp to search for. Deletes all records >= the endTimeStamp.</param>
        /// <returns></returns>
        Task DeleteLogByDate(DateTime endTimeStamp);
    }
}
