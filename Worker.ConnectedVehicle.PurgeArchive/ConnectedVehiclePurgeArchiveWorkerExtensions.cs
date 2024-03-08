// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using Econolite.Ode.Worker.ConnectedVehicle.PurgeArchive;

namespace Econolite.Ode.Worker.ConnectedVehicle.ArchiveLogger
{
    public static class ConnectedVehicleArchiveLoggerWorkerExtensions
    {
        public static IServiceCollection AddConnectedVehiclePurgeArchiveWorker(this IServiceCollection services)
        {
            //every minute kafka topic consumer
            services
                .AddTransient<IPayloadSpecialist<string>, JsonPayloadSpecialist<string>>()
                .AddTransient<Func<byte[], string>>(_ => _ => string.Empty)
                .AddTransient<IConsumeResultFactory<string, string>, ConsumeResultFactory<string, string>>()
                .AddTransient<IConsumer<string, string>, Consumer<string, string>>();

            services.AddMessaging();

            services.AddConnectedVehicleArchiveLogRepository();
            services.AddConnectedVehicleConfigRepository();

            services.AddConnectedVehicleArchiveService();
            services.AddConnectedVehicleConfigService();

            services.AddHostedService<ConnectedVehiclePurgeArchiveWorker>();

            return services;
        }
    }
}
