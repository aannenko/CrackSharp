using CrackSharp.Api.Services;
using CrackSharp.Api.Services.Des;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache(options => options.SizeLimit =
    builder.Configuration.GetValue("Decryption:CacheSizeBytes", 52_428_800 /* 50 MB */));

builder.Services.AddSingleton(typeof(DecryptionMemoryCache<,>));
builder.Services.AddSingleton<DesBruteForceDecryptionService>();
builder.Services.AddSingleton<DesEncryptionService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CrackSharp.Api v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
