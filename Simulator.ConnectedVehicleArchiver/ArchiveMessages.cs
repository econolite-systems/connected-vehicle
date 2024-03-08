// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Simulator.ConnectedVehicleArchiver
{ 
    public class ArchiveMessages : IHostedService
    {
        private readonly ILogger<ArchiveMessages> _logger;
        private IConnectedVehicleArchiveService _cvArchiveService;
        private IConnectedVehicleStatusService _cvStatusService;

        public ArchiveMessages(IServiceProvider serviceProvider, ILogger<ArchiveMessages> logger)
        {
            _logger = logger;
            _cvArchiveService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleArchiveService>();
            _cvStatusService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConnectedVehicleStatusService>();
        }
        public async Task StartAsync(CancellationToken stoppingToken)
        {
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            _logger.LogInformation(String.Format("Archiving date range: {0} - {1}", startDate, endDate));

            //TODO:  ko - test if a find will work if we change archiving to copy from mongo
            //var logs = await _cvStatusService.Find(startDate, endDate);

            var totals = await _cvStatusService.GetTotalsByRepositoryTypeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
