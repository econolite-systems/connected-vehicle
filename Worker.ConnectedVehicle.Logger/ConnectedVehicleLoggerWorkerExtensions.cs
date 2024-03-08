// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle.Messaging;
using System.Text.Json;

namespace Econolite.Ode.Worker.ConnectedVehicle.Logger
{
    public static class ConnectedVehicleLoggerWorkerExtensions
    {
        public static IServiceCollection AddConnectedVehicleLoggerWorker(this IServiceCollection services)
        {
            //Note:  These messages can have different document formats so we will use a generic jsonDocument type
            services
                .AddTransient<Func<byte[], string>>(_ => _ => string.Empty)
                .AddTransient<IConsumeResultFactory<string, JsonDocument>, ConnectedVehicleConsumeResultFactory>()
                .AddTransient<IConsumer<string, JsonDocument>, Consumer<string, JsonDocument>>();

            services.AddMessaging();

            services.AddConnectedVehicleLogRepository();
            services.AddConnectedVehicleConfigRepository();

            services.AddConnectedVehicleLoggerService();

            services.AddHostedService<ConnectedVehicleBSMLogger>();
            services.AddHostedService<ConnectedVehicleTIMLogger>();
            services.AddHostedService<ConnectedVehicleSPATLogger>();
            services.AddHostedService<ConnectedVehicleSRMLogger>();

            return services;
        }
    }
}
