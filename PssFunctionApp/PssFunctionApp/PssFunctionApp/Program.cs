using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using PssFunctionApp.Reponsitory;
using PssFunctionApp.Reponsitory.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<ServiceClient>(sp =>
        {
            string connectionString = Environment.GetEnvironmentVariable("DataverseConnectionString");
            return new ServiceClient(connectionString);
        });
        services.AddScoped<IUnitReponsitory, UnitReponsitory>();
    })
    .Build();

host.Run();
