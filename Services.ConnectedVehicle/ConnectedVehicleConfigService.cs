// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Helpers.Exceptions;
using Econolite.Ode.Models.ConnectedVehicle.Dto;
using Econolite.Ode.Repository.ConnectedVehicle;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public class ConnectedVehicleConfigService : IConnectedVehicleConfigService
    {
        private readonly IConnectedVehicleConfigRepository _cvConfigRepository;
        private readonly ILogger<ConnectedVehicleConfigService> _logger;

        public ConnectedVehicleConfigService(IConnectedVehicleConfigRepository cvConfigRepository, ILogger<ConnectedVehicleConfigService> logger)
        {
            _cvConfigRepository = cvConfigRepository;
            _logger = logger;
        }

        public async Task<ConnectedVehicleConfigDto?> GetFirstAsync()
        {
            var list = await _cvConfigRepository.GetAllAsync();
            return list.Select(e => e.AdaptToDto()).FirstOrDefault();
        }

        public async Task<ConnectedVehicleConfigDto> AddAsync(ConnectedVehicleConfigAdd add)
        {
            var cvs = add.AdaptToConnectedVehicleConfig();
            cvs.Id = Guid.NewGuid();

            //enforce that  only one record can exist
            var list = await _cvConfigRepository.GetAllAsync();
            if (list.Any())
            {
                throw new AddException("Connected Vehicle configuration already exists");
            }

            _cvConfigRepository.Add(cvs);

            var (success, _) = await _cvConfigRepository.DbContext.SaveChangesAsync();

            return cvs.AdaptToDto();
        }

        public async Task<ConnectedVehicleConfigDto?> UpdateAsync(ConnectedVehicleConfigUpdate update)
        {
            try
            {
                var pcs = await _cvConfigRepository.GetByIdAsync(update.Id);
                var updated = update.AdaptTo(pcs);

                _cvConfigRepository.Update(updated);

                var (success, errors) = await _cvConfigRepository.DbContext.SaveChangesAsync();
                if (!success && !string.IsNullOrWhiteSpace(errors)) throw new UpdateException(errors);
                return updated.AdaptToDto();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _cvConfigRepository.Remove(id);
                var (success, errors) = await _cvConfigRepository.DbContext.SaveChangesAsync();
                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }
    }
}
