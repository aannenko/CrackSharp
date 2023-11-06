using CrackSharp.Api.Common;
using CrackSharp.Api.Common.Services;
using CrackSharp.Api.Des.Endpoints;
using CrackSharp.Api.Des.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
builder.Services.AddSingleton(typeof(AwaitableMemoryCache<,>));
builder.Services.AddSingleton<DesBruteForceDecryptionService>();
builder.Services.AddSingleton<DesEncryptionService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CrackSharp.Api v1"));
}

app.UseHttpsRedirection();

DesEndpoints.Map(app);

app.Run();
