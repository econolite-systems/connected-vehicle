// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Diagnostics;
using Econolite.Ode.Messaging;
using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using System.Text.Json;
using Econolite.Ode.Monitoring.Metrics;

namespace Econolite.Ode.Worker.ConnectedVehicle.ArchiveLogger;

public class ConnectedVehicleBSMLogger : BackgroundService
{
    private readonly IConsumer<string, JsonDocument> _consumer;
    private readonly ILogger<ConnectedVehicleBSMLogger> _logger;
    private readonly IConnectedVehicleArchiveService _archiveService;
    private readonly IMetricsCounter _loopCounter;

    public ConnectedVehicleBSMLogger(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ConnectedVehicleBSMLogger> logger,
        IConsumer<string, JsonDocument> consumer,
        IMetricsFactory metricsFactory
    )
    {
        var serviceScope = serviceProvider.CreateScope();

        _consumer = consumer;
        _logger = logger;

        _archiveService = serviceScope.ServiceProvider.GetRequiredService<IConnectedVehicleArchiveService>();

        var topic = configuration[Consts.TOPICS_CV_BSM] ?? throw new NullReferenceException($"{Consts.TOPICS_CV_BSM} missing in configuration");
        _consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed topic {@}", topic);

        _loopCounter = metricsFactory.GetMetricsCounter("Archive Logger BSM");
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
                        var result = _consumer.Consume(stoppingToken);
                        Debug.Assert(result is not null);
                        try
                        {
                            var task = result.Value switch
                            {
                                JsonDocument status => _archiveService.ProcessMessageAsync(status, ConnectedVehicleMessageTypeEnum.BSM),
                                _ => Task.CompletedTask
                            };

                            await task;
                            _consumer.Complete(result);

                            _loopCounter.Increment();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unhandled exception while processing: {@MessageType}", result.Type);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception thrown while trying to consume BSM messages");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker.ConnectedVehicle stopping");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception thrown while processing connected vehicle messages");
            }
        }, stoppingToken);
    }
}