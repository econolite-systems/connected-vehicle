// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Extensions;
using Simulator.ConnectedVehicleProducer;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Simulator Connected Vehicle Producer"; }, (builder, services) =>
{
    //NOTE: we want the message to have no key and an undefined message type
    services.AddTransient<IProducer<string, object>, Producer<string, object>>();

    services.AddHostedService<ConnectedVehicleMessageProducer>();

    services.AddMessaging();
});
