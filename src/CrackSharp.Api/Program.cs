using CrackSharp.Api.Actions;
using CrackSharp.Api.Extensions;
using CrackSharp.Api.Serialization;
using CrackSharp.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(
    options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddOpenApi();

builder.Services.AddValidation();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(typeof(Log<>));

builder.Services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
builder.Services.AddSingleton(typeof(AwaitableMemoryCache<,>));

builder.Services.AddDesServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

app.MapDesEndpoints();

await app.RunAsync().ConfigureAwait(false);
