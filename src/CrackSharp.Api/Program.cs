using CrackSharp.Api.Actions;
using CrackSharp.Api.Extensions;
using CrackSharp.Api.Serialization;
using CrackSharp.Api.Services;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(static (context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.ConfigureHttpJsonOptions(
            options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

        services.AddValidation();
        services.AddProblemDetails();

        services.AddSingleton(typeof(Log<>));

        services.AddMemoryCache(options => options.SizeLimit = context.Configuration.GetCacheSizeLimit());
        services.AddSingleton(typeof(AwaitableMemoryCache<,>));

        services.AddDesServices();
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
