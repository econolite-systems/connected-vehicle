// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Services.ConnectedVehicle;
using System.Diagnostics;

namespace Econolite.Ode.Worker.ConnectedVehicle.PurgeLog;

public class ConnectedVehiclePurgeLogWorker : BackgroundService
{
    private readonly ILogger<ConnectedVehiclePurgeLogWorker> _logger;
    private readonly IConnectedVehicleLoggerService _loggerService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IConsumer<string, string> _everyMinConsumer;
    private readonly IMetricsCounter _loopCounter;

    public ConnectedVehiclePurgeLogWorker(
        IServiceProvider serviceProvider,
        ILogger<ConnectedVehiclePurgeLogWorker> logger,
        IConsumer<string, string> consumer,
        IConfiguration configuration,
        IMetricsFactory metricsFactory
    )
    {
        _logger = logger;
        _everyMinConsumer = consumer;

        var topic = configuration[Consts.TOPICS_CV_EVERY_MIN];
        _everyMinConsumer.Subscribe(topic ?? throw new NullReferenceException($"{Consts.TOPICS_CV_EVERY_MIN} topic not in configuration."));
        _logger.LogInformation("Subscribed topic {@}", topic);

        _loopCounter = metricsFactory.GetMetricsCounter("Purge Log");

        var serviceScope = serviceProvider.CreateScope();
        _loggerService = serviceScope.ServiceProvider.GetRequiredService<IConnectedVehicleLoggerService>();
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
                                { } timerTick => PurgeDataAsync(stoppingToken),
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
                _logger.LogInformation("Worker.ConnectedVehicle.PurgeLog stopping");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception thrown while purging log data");
            }
        }, stoppingToken);
    }

    /// <summary>
    /// Main purge process that is only allowed to run one at a time.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task PurgeDataAsync(CancellationToken cancellationToken)
    {
        //only allow one process to run at a time
        if (await _semaphore.WaitAsync(0, cancellationToken))
        {
            try
            {
                //delete from the archive
                await _loggerService.PurgeDataAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        else
        {
            _logger.LogWarning("Did not complete previous purge");
        }
    }
}