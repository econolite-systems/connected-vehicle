// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;

namespace Econolite.Ode.Worker.ConnectedVehicle.PurgeLog
{
    public static class ConnectedVehicleArchiveLoggerWorkerExtensions
    {
        public static IServiceCollection AddConnectedVehiclePurgeLogWorker(this IServiceCollection services)
        {
            //every minute kafka topic consumer
            services
                .AddTransient<IPayloadSpecialist<string>, JsonPayloadSpecialist<string>>()
                .AddTransient<Func<byte[], string>>(_ => _ => string.Empty)
                .AddTransient<IConsumeResultFactory<string, string>, ConsumeResultFactory<string, string>>()
                .AddTransient<IConsumer<string, string>, Consumer<string, string>>();
            
            services.AddMessaging();

            services.AddConnectedVehicleLogRepository();
            services.AddConnectedVehicleConfigRepository();

            services.AddConnectedVehicleLoggerService();
            services.AddConnectedVehicleConfigService();

            services.AddHostedService<ConnectedVehiclePurgeLogWorker>();

            return services;
        }
    }
}
