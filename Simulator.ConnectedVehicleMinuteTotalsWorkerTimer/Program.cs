using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simulator.ConnectedVehicleMessageTypeMinuteTotalsWorkerTimer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builderContext, services) =>
    {
        services.AddMongo(options =>
        {
            options.DbConnection = builderContext.Configuration.GetConnectionString("Mongo");
            options.DbName = builderContext.Configuration["Mongo:DbName"];
        });
        services.AddConnectedVehicleLogRepository();
        services.AddConnectedVehicleLoggerService();
        services.AddHostedService<ConnectedVehicleMessageTypeMinuteTotalsWorker>();
    })
    .Build();


var loggingfactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

var _logger = loggingfactory?.CreateLogger("Main");
var assembly = System.Reflection.Assembly.GetExecutingAssembly();
_logger?.LogInformation("------------------------------------------------------");
_logger?.LogInformation("---------------------- Starting ----------------------");
_logger?.LogInformation("------------------------------------------------------");
_logger?.LogInformation("Assembly {$assembly} ...", assembly);
var hostingenvironment = host.Services.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
_logger?.LogInformation("Hosting Environment Name {@Hostingenvironment}", hostingenvironment?.EnvironmentName);

await host.RunAsync();