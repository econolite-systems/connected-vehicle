// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public interface IConnectedVehicleStatusService
    {

        /// <summary>
        ///     Finds Connected Vehicle Logs with a timestamp between the
        ///     given start and end dates. If the end date is not given, returns all statuses with a timestamp
        ///     after the given start date.
        /// </summary>
        /// <param name="startDate">Mandatory start date for filtering log entries</param>
        /// <param name="endDate">Optional end date for filtering log entries</param>
        /// <returns>A list of connected vehicle message objects</returns>
        Task<List<ConnectedVehicleMessageDocument>> FindAsync(DateTime startDate, DateTime? endDate);

        /// <summary>
        /// Calculates the total number and size of connected vehicle messages grouped by each connected vehicle message type (ConnectedVehicleMessageTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleMessageTypeCountAndSize>> GetTotalsByMessageTypeAsync();

        /// <summary>
        /// Calculates the total number and size of connected vehicle messages by repository type (ConnectedVehicleRepositoryTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryTypeAsync();

        /// <summary>
        /// Counts the last 60 mins of connected vehicle messages by type (ConnectedVehicleMessageTypeEnum)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleMessageTypeCount>> GetLastHourTotalsByMessageTypeAsync();

        /// <summary>
        /// Counts the total number of connected vehicle messages
        /// </summary>
        /// <returns></returns>
        Task<long> GetTotalMessageCountAsync();

        /// <summary>
        /// Calculates the totals by intersection and type (ConnectedVehicleMessageTypeEnum - SPAT and SRM)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ConnectedVehicleIntersectionTypeCountAndSize>> GetIntersectionTotalsByMessageTypeAsync();

    }
}
