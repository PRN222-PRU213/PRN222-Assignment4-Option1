using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN222_Assignment4_Option1.BusinessLogic.Services;
using PRN222_Assignment4_Option1.DataAccess.Data;
using PRN222_Assignment4_Option1.DataAccess.Repositories;

namespace PRN222_Assignment4_Option1.BusinessLogic.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExchangeRateServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Server=.;Database=ExchangeRateDb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<IExchangeRateApiService>(_ =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.frankfurter.dev/")
            };
            return new ExchangeRateApiService(httpClient);
        });

        return services;
    }
}

