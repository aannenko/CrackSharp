using CrackSharp.Api;
using CrackSharp.Api.Endpoints;
using CrackSharp.Api.Services;
using CrackSharp.Api.Services.Des;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache(options => options.SizeLimit = builder.Configuration.GetCacheSizeLimit());
builder.Services.AddSingleton(typeof(DecryptionMemoryCache<,>));
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
