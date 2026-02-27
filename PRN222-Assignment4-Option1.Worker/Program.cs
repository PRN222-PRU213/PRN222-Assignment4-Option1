using PRN222_Assignment4_Option1.BusinessLogic.Extensions;
using PRN222_Assignment4_Option1.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddExchangeRateServices(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
