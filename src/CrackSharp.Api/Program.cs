using CrackSharp.Api.Actions;
using CrackSharp.Api.Extensions;
using CrackSharp.Api.Serialization;
using CrackSharp.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication();

var services = builder.Services;

services.AddApplicationInsightsTelemetryWorkerService();
services.ConfigureFunctionsApplicationInsights();

services.ConfigureHttpJsonOptions(static options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

services.AddProblemDetails();

services.AddSingleton(typeof(Log<>));

services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
services.AddSingleton(typeof(AwaitableMemoryCache<,>));

services.AddDesServices();

var host = builder.Build();

await host.RunAsync().ConfigureAwait(false);
