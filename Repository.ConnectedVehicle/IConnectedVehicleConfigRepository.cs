// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Persistence.Common.Repository;
using Econolite.Ode.Models.ConnectedVehicle.Db;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public interface IConnectedVehicleConfigRepository : IRepository<ConnectedVehicleConfig, Guid>
    {
    }
}
