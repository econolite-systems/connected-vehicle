// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Models.ConnectedVehicle.Dto;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public interface IConnectedVehicleConfigService
    {
        Task<ConnectedVehicleConfigDto?> GetFirstAsync();
        Task<ConnectedVehicleConfigDto> AddAsync(ConnectedVehicleConfigAdd add);
        Task<ConnectedVehicleConfigDto?> UpdateAsync(ConnectedVehicleConfigUpdate update);
        Task<bool> DeleteAsync(Guid id);
    }
}
