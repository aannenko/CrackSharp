using CrackSharp.Api.Common;
using CrackSharp.Api.Common.Services;
using CrackSharp.Api.Des.Endpoints;
using CrackSharp.Api.Des.Services;
using Microsoft.AspNetCore.Routing.Constraints;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(
    options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.Configure<RouteOptions>(
    options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));

builder.Services.AddSingleton<ISerializerDataContractResolver, JsonSerializerDataContractResolver>(
    _ => new(new() { TypeInfoResolver = AppJsonSerializerContext.Default }));

builder.Services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
builder.Services.AddSingleton(typeof(AwaitableMemoryCache<,>));
builder.Services.AddSingleton<DesBruteForceDecryptionService>();
builder.Services.AddSingleton<DesEncryptionService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDesApi();

app.Run();
