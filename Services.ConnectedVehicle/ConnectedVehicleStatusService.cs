// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle.Status;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;
using Econolite.Ode.Repository.ConnectedVehicle;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public class ConnectedVehicleStatusService: IConnectedVehicleStatusService
    {
        private readonly IConnectedVehicleLogRepository _cvLogRepo;
        private readonly IConnectedVehicleArchiveLogRepository _cvArchiveLogRepo;
        private readonly ILogger<ConnectedVehicleStatusService> _logger;
       

        public ConnectedVehicleStatusService(IConnectedVehicleLogRepository cvLogRepo, IConnectedVehicleArchiveLogRepository cvArchiveLogRepo, ILogger<ConnectedVehicleStatusService> logger)
        {
            _cvLogRepo = cvLogRepo;
            _cvArchiveLogRepo = cvArchiveLogRepo;

            _logger = logger;
        }

        public async Task<List<ConnectedVehicleMessageDocument>> FindAsync(DateTime startDate, DateTime? endDate)
        {
            var result = await _cvLogRepo.Find(startDate, endDate);

            return result;
        }

        public async Task<IEnumerable<ConnectedVehicleMessageTypeCountAndSize>> GetTotalsByMessageTypeAsync()
        {
            var result =await _cvLogRepo.GetTotalsByMessageType();

            return result;
        }
        
        public async Task<IEnumerable<ConnectedVehicleRepositoryTypeCountAndSize>> GetTotalsByRepositoryTypeAsync()
        {
            //get the working repo data
            var logResult = await _cvLogRepo.GetTotalsByRepositoryType();

            //get the archive repo data
            var summaryResult = await _cvArchiveLogRepo.GetTotalsByRepositoryType();

            var result = logResult.Union(summaryResult);

            return result;
        }

        public async Task<IEnumerable<ConnectedVehicleMessageTypeCount>> GetLastHourTotalsByMessageTypeAsync()
        {
            var result = await _cvLogRepo.GetLastHourTotalsByMessageType();

            return result;
        }

        public async Task<long> GetTotalMessageCountAsync()
        {
            var result = await _cvLogRepo.GetTotalMessageCount();

            return result;
        }

        public async Task<IEnumerable<ConnectedVehicleIntersectionTypeCountAndSize>> GetIntersectionTotalsByMessageTypeAsync()
        {
            var totals =  await _cvLogRepo.GetIntersectionTotals();
            var totalsWithIntersectionNames = totals.Select(t => t.AdaptToIntersectionName());

            return totalsWithIntersectionNames;

        }
    }
}
