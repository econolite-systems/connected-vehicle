// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Extensions;
using Simulator.ConnectedVehicleProducer;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Simulator CV Every Minute Producer"; }, (builder, services) =>
{
    services.AddTransient<IProducer<string, string>, Producer<string, string>>();
    services.AddHostedService<ConnectedVehicleEveryMinuteMessageProducer>();
    services.AddMessaging();
});