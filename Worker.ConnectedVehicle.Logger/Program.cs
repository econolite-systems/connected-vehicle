// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Worker.ConnectedVehicle.Logger;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Connected Vehicle Logger"; }, (builder, services) =>
{
    services.AddMongo();
    services.AddConnectedVehicleLoggerWorker();
}, (_, checksBuilder) => checksBuilder.AddMongoDbHealthCheck());
