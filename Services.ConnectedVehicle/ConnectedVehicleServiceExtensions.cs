// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.ConnectedVehicle
{
    public static class ConnectedVehicleServiceExtensions
    {
        public static IServiceCollection AddConnectedVehicleStatusService(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleStatusService, ConnectedVehicleStatusService>();
            return services;
        }

        public static IServiceCollection AddConnectedVehicleArchiveService(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleArchiveService, ConnectedVehicleArchiveService>();
            return services;
        }

        public static IServiceCollection AddConnectedVehicleConfigService(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleConfigService, ConnectedVehicleConfigService>();
            return services;
        }

        public static IServiceCollection AddConnectedVehicleLoggerService(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleLoggerService, ConnectedVehicleLoggerService>();
            return services;
        }
    }
}
