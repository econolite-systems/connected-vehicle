using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Simulator.ConnectedVehicleMessageTypeMinuteTotalsWorkerTimer
{
    /// <summary>
    /// Lets you test the minute totals calculations without having a Kubernetes cronjob.  Uses a Timer to execute the calculations every minute.
    /// </summary>
    public class ConnectedVehicleMessageTypeMinuteTotalsWorker : BackgroundService
    {
        private readonly ILogger<ConnectedVehicleMessageTypeMinuteTotalsWorker> _logger;
        private readonly IConnectedVehicleLoggerService _connectedVehicleLoggerService;
        private Timer? _timer;

        public ConnectedVehicleMessageTypeMinuteTotalsWorker(IServiceProvider serviceProvider, ILogger<ConnectedVehicleMessageTypeMinuteTotalsWorker> logger)
        {
            _connectedVehicleLoggerService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleLoggerService>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(1);

                _timer = new Timer(async (e) =>
                {
                    await UpdateStaleCounts();
                }, null, startTimeSpan, periodTimeSpan);
                return Task.CompletedTask;
            });
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task UpdateStaleCounts()
        {
            try
            {
                //update the on-demand materialized view with a new count for each minute that has passed since the last update
                await _connectedVehicleLoggerService.UpdateConnectedVehicleMessageTypeMinuteTotals();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while updating connected vehicle minute totals stale counts");
            }
        }

    }
}
