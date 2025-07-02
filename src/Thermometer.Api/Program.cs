using Thermometer.Services;
using Thermometer.Ports;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("DEVICE_PORT") ?? "8080"));

builder.Services.AddControllers();
builder.Services.AddSingleton<ServerCommunicationProtocolHttpAdapter>();
builder.Services.AddSingleton<IThermometerService, ThermometerService>();

builder.Services.AddHostedService(provider => 
    (ThermometerService)provider.GetRequiredService<IThermometerService>());

var app = builder.Build();

// Map controller routes
app.MapControllers();

app.Run();