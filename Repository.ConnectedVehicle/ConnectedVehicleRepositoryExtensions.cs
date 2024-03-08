// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public static class ConnectedVehicleRepositoryExtensions
    {
        public static IServiceCollection AddConnectedVehicleConfigRepository(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleConfigRepository, ConnectedVehicleConfigRepository>();
            return services;
        }

        public static IServiceCollection AddConnectedVehicleLogRepository(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleLogRepository, ConnectedVehicleLogRepository>();
            return services;
        }

        public static IServiceCollection AddConnectedVehicleArchiveLogRepository(this IServiceCollection services)
        {
            services.AddScoped<IConnectedVehicleArchiveLogRepository, ConnectedVehicleArchiveLogRepository>();
            return services;
        }
    }
}
