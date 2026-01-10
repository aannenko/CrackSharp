using CrackSharp.Api.Common;
using CrackSharp.Api.Common.Logging;
using CrackSharp.Api.Common.Services;
using CrackSharp.Api.Des.Endpoints;
using CrackSharp.Api.Des.Services;
using Microsoft.AspNetCore.Routing.Constraints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(
    options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.Configure<RouteOptions>(
    options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));

builder.Services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
builder.Services.AddSingleton(typeof(Log<>));
builder.Services.AddSingleton(typeof(AwaitableMemoryCache<,>));
builder.Services.AddSingleton<DesBruteForceDecryptionService>();
builder.Services.AddSingleton<DesEncryptionService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnetcore/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDesApi();

app.Run();
