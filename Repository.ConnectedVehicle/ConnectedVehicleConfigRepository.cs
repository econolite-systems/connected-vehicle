// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Econolite.Ode.Models.ConnectedVehicle.Db;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public class ConnectedVehicleConfigRepository : GuidDocumentRepositoryBase<ConnectedVehicleConfig>, IConnectedVehicleConfigRepository
    {
        public ConnectedVehicleConfigRepository(IMongoContext context, ILogger<ConnectedVehicleConfigRepository> logger) : base(context, logger)
        {
        }
    }
}
