// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Models.ConnectedVehicle.Db;
using Econolite.Ode.Models.ConnectedVehicle.Messaging;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Repository.ConnectedVehicle.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public class ConnectedVehicleLoggerService: IConnectedVehicleLoggerService
    {

        private readonly IConnectedVehicleLogRepository _cvLogStorage;
        private readonly ILogger<ConnectedVehicleLoggerService> _logger;
        private readonly IConnectedVehicleConfigRepository _cvConfigStorage;
        private readonly UserEventFactory _userEventFactory;

        public ConnectedVehicleLoggerService(IServiceProvider serviceProvider, ILogger<ConnectedVehicleLoggerService> logger, UserEventFactory userEventFactory)
        {
            _logger = logger;
            _userEventFactory = userEventFactory;

            var serviceScope = serviceProvider.CreateScope();
            _cvLogStorage = serviceScope.ServiceProvider.GetRequiredService<IConnectedVehicleLogRepository>();
            _cvConfigStorage = serviceScope.ServiceProvider.GetRequiredService<IConnectedVehicleConfigRepository>();
        }

        #region message logging consumer worker

        public async Task LogCvMessageAsync(ConnectedVehicleMessageDocument message, ConnectedVehicleMessageTypeEnum type)
        {
            _logger.LogDebug($"Logging connected vehicle message to the log.");
            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Processed vehicle data of type: {0}", type)));
            await _cvLogStorage.InsertAsync(message);
        }

        public Task LogNonParseableMessageAsync(NonParseableConnectedVehicleMessage nonParseable, ConnectedVehicleMessageTypeEnum type)
        {
            _logger.LogError("Unable to parse connected vehicle message: {}", nonParseable);
            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("Received non-parseable vehicle data of type: {0}, message: {1}", type, nonParseable.Data)));
            return Task.CompletedTask;
        }

        public Task LogUnknownMessageAsync(UnknownConnectedVehicleMessage unknown, ConnectedVehicleMessageTypeEnum type)
        {
            _logger.LogError("Received unknown connected vehicle message: {}", unknown);
            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("Received unknown vehicle data of type: {0}, message: {1}", type, unknown.Data)));
            return Task.CompletedTask;
        }

        public Task ProcessMessageAsync(JsonDocument status, ConnectedVehicleMessageTypeEnum type)
        {
            var elements = status.RootElement;
            JsonElement element;

            //All of the jsonDocuments seem to deserialize equally so adding a property I can look for to differentiate between the documents

            if (elements.TryGetProperty("UnErrorType", out element))
            {
                try
                {
                    var unknown = status.Deserialize<UnknownConnectedVehicleMessage>()!;
                    return LogUnknownMessageAsync(unknown, type);
                }
                catch (Exception e)
                {
                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Unable to deserialize UnknownConnectedVehicleMessage: {0}", status)));
                    _logger.LogError(e, "Unable to deserialize UnknownConnectedVehicleMessage : {}", status);
                }
            }
            else if (elements.TryGetProperty("NpErrorType", out element))
            {
                try
                {
                    var nonParseable = status.Deserialize<NonParseableConnectedVehicleMessage>()!;
                    return LogNonParseableMessageAsync(nonParseable, type);
                }
                catch (Exception e)
                {
                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Unable to deserialize NonParseableConnectedVehicleMessage: {0}", status)));
                    _logger.LogError(e, "Unable to deserialize NonParseableConnectedVehicleMessage : {}", status);
                }
            }
            else
            {
                //we have good data so log it
                return LogCvMessageAsync(status.AdaptToDocument(type), type);
            }

            // Reached an exception
            return Task.CompletedTask;
        }

        #endregion

        #region summary calculation workers

        public async Task UpdateConnectedVehicleMessageTypeMinuteTotalsAsync()
        {
            var startDate = await _cvLogStorage.UpdateConnectedVehicleMessageTypeMinuteTotals();

            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Updating Connected Vehicle Minute Totals since {0}", startDate)));
        }

        #endregion

        #region purge data worker

        public async Task PurgeDataAsync()
        {
            ConnectedVehicleConfig? config = null;
            try
            {
                //get the purge settings from the config
                var configs = _cvConfigStorage.GetAll();
                if (configs.Any())
                {
                    //There should only be one config
                    config = configs.First();

                    //just want the hrs/mins from the config
                    var now = DateTime.UtcNow;
                    var startTime = new DateTime(now.Year, now.Month, now.Day, config.StartTime.Hour, config.StartTime.Minute, 0, DateTimeKind.Utc);
                    var endTime = new DateTime(now.Year, now.Month, now.Day, config.EndTime.Hour, config.EndTime.Minute, 59, DateTimeKind.Utc);

                    _logger.LogDebug("Checking if time is ok to purge. now={now} >= startTime={startTime} <= endTime={endTime}", now, startTime, endTime);

                    if (now >= startTime && now <= endTime)
                    {
                        _logger.LogDebug("Start purge by OnlineStorageType: {type}", config.OnlineStorageType);

                        switch (config.OnlineStorageType)
                        {
                            case ConnectedVehicleStorageType.Age:
                                var days = config.OnlineDays;
                                var endDate = DateTime.UtcNow.AddDays(-days);
                                //searching for >=Timestamp; so we want to make it at the last second of the day 
                                var endTimeStamp = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, DateTimeKind.Utc);
                                _logger.LogDebug($"Purge by age. days={days}, " + $" endDate= {endDate}" + $" endTimestamp= {endTimeStamp}");
                                await _cvLogStorage.DeleteLogByDate(endTimeStamp);
                                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Purged Connected Vehicle data older than: {0}", endTimeStamp)));

                                break;
                            case ConnectedVehicleStorageType.Size:
                                var size = config.OnlineSize;
                                _logger.LogDebug($"Purge by size. size={size}");
                                await _cvLogStorage.DeleteLogBySize(size);
                                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Purged Connected Vehicle data to shrink repo to: {0}MB", size / 1024.0 / 1024.0)));
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Unable to purge data using config: {0}", config?.Id)));

                _logger.LogError(ex, "Unable to purge data.");
            }
        }
        #endregion
    }
}
