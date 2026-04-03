using Microsoft.EntityFrameworkCore;

namespace PortfolioManagement.Api.Infrastructure.Portfolio;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PortfolioDbConnection")));
        return services;
    }
}