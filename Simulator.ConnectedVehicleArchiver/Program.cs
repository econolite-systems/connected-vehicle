// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using Simulator.ConnectedVehicleArchiver;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Simulator Connected Vehicle Archiver"; }, (builder, services) =>
{
    services.AddMongo();

    services.AddMessaging();

    services.AddConnectedVehicleConfigRepository();
    services.AddConnectedVehicleLogRepository();
    services.AddConnectedVehicleArchiveLogRepository();

    services.AddConnectedVehicleStatusService();
    services.AddConnectedVehicleArchiveService();
    services.AddConnectedVehicleLoggerService();

    //test the purge 
    //services.AddHostedService<ArchiveMessages>();

    //test the insert into archive
    services.AddHostedService<TestAzureBlobManipulations>();
});
