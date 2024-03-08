// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Worker.ConnectedVehicle.PurgeLog;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Connected Vehicle Purge Log"; }, (builder, services) =>
{
    services.AddMongo();
    services.AddConnectedVehiclePurgeLogWorker();
}, (_, checksBuilder) => checksBuilder.AddMongoDbHealthCheck());
