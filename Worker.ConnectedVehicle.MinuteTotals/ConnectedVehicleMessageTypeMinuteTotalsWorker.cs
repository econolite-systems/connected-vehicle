// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Messaging;
using Econolite.Ode.Services.ConnectedVehicle;
using System.Diagnostics;
using Econolite.Ode.Monitoring.Metrics;

namespace Econolite.Ode.Worker.ConnectedVehicle.MinuteTotals
{
    public class ConnectedVehicleMessageTypeMinuteTotalsWorker : BackgroundService
    {
        private readonly ILogger<ConnectedVehicleMessageTypeMinuteTotalsWorker> _logger;
        private readonly IConnectedVehicleLoggerService _connectedVehicleLoggerService;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IConsumer<string, string> _everyMinConsumer;
        private readonly IMetricsCounter _loopCounter;

        public ConnectedVehicleMessageTypeMinuteTotalsWorker(IServiceProvider serviceProvider, ILogger<ConnectedVehicleMessageTypeMinuteTotalsWorker> logger,
            IConsumer<string, string> consumer, IConfiguration configuration, IMetricsFactory metricsFactory)
        {
            _connectedVehicleLoggerService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleLoggerService>();
            _logger = logger;
            _everyMinConsumer = consumer;

            var topic = configuration[Consts.TOPICS_CV_EVERY_MIN] ?? throw new NullReferenceException($"{Consts.TOPICS_CV_EVERY_MIN} missing in configuration");
            _everyMinConsumer.Subscribe(topic);
            _logger.LogInformation("Subscribed topic {@}", topic);

            _loopCounter = metricsFactory.GetMetricsCounter("Minute Totals");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _everyMinConsumer.Consume(stoppingToken);
                            Debug.Assert(result is not null);
                            try
                            {
                                var task = result.Value switch
                                {
                                    string timerTick => UpdateTotalsAsync(stoppingToken),
                                    _ => Task.CompletedTask
                                };

                                await task;
                                _everyMinConsumer.Complete(result);

                                _loopCounter.Increment();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Unhandled exception while processing: {@MessageType}", result.Type);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Exception thrown while trying to consume every minute messages");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Worker.ConnectedVehicle stopping");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Exception thrown while updating connected vehicle minute totals stale counts");
                }
            }, stoppingToken);
        }

        /// <summary>
        /// Main update totals process that is only allowed to run one at a time.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UpdateTotalsAsync(CancellationToken cancellationToken)
        {
            //only allow one process to run at a time
            if (await _semaphore.WaitAsync(0, cancellationToken))
            {
                try
                {
                    //update the on-demand materialized view with a new count for each minute that has passed since the last update
                    _logger.LogDebug($"Running UpdateConnectedVehicleMessageTypeMinuteTotals.");
                    await _connectedVehicleLoggerService.UpdateConnectedVehicleMessageTypeMinuteTotalsAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            else
            {
                _logger.LogWarning("Did not complete previous totals update");
            }
        }
    }
}
