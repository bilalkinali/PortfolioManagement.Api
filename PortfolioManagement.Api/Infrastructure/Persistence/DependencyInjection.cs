using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;
using PortfolioManagement.Api.Features.Trades.AddTrade;
using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PortfolioDbConnection")));

        services.AddDataProtection();

        services
            .AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<PortfolioDbContext>()
            .AddDefaultTokenProviders();

        // Add-Migration InitialMigration -Context PortfolioDbContext -OutputDir Infrastructure/Migrations
        // Update-Database -Context PortfolioDbContext

        services.AddScoped<CreatePortfolioHandler>();
        services.AddScoped<AddTradeHandler>();

        return services;
    }
}