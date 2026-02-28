using PRN222_Assignment4_Option1.BusinessLogic.Extensions;
using PRN222_Assignment4_Option1.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddExchangeRateServices(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
