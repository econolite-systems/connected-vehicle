// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Models.ConnectedVehicle.Messaging;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using System.Text.Json;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public interface IConnectedVehicleLoggerService
    {

        #region message logging worker

        /// <summary>
        /// Log a connected vehicle message document to the repository
        /// </summary>
        Task LogCvMessageAsync(ConnectedVehicleMessageDocument message, ConnectedVehicleMessageTypeEnum type);

        /// <summary>
        /// Log a non-parseable connected vehicle message to the logger
        /// </summary>
        Task LogNonParseableMessageAsync(NonParseableConnectedVehicleMessage nonParseable, ConnectedVehicleMessageTypeEnum type);

        /// <summary>
        /// Log an unknown connected vehicle message to the logger
        /// </summary>
        Task LogUnknownMessageAsync(UnknownConnectedVehicleMessage unknown, ConnectedVehicleMessageTypeEnum type);

        /// <summary>
        /// Takes a JsonDocument message and determines where it should be logged to.  If the message is UnknownConnectedVehicleMessage or a NonParseableConnectedVehicleMessage it will be logged to the logger.
        /// All other message types will be logged to Mongo.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task ProcessMessageAsync(JsonDocument status, ConnectedVehicleMessageTypeEnum type);

        #endregion

        #region summary calculation workers

        /// <summary>
        /// Updates the count of the messages per type per M/D/Y H:M. Queries the data for any logs greater than the lastModified date so it only get the latest changes.
        /// Takes the new counts and adds them to the existing count from the last modification.
        /// </summary>
        /// <returns></returns>
        Task UpdateConnectedVehicleMessageTypeMinuteTotalsAsync();

        #endregion

        #region purge data worker

        /// <summary>
        /// Deletes records out of the connected vehicle log storage based on the config settings stored in mongo.
        /// </summary>
        /// <returns></returns>
        Task PurgeDataAsync();

        #endregion

    }
}
