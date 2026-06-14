using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Features.Auth.Login;
using PortfolioManagement.Api.Features.Auth.Me;
using PortfolioManagement.Api.Features.Auth.Register;
using PortfolioManagement.Api.Features.Instruments.SearchInstruments;
using PortfolioManagement.Api.Features.Instruments.SearchInstruments.Proxy;
using PortfolioManagement.Api.Features.MarketData;
using PortfolioManagement.Api.Features.MarketData.Finnhub;
using PortfolioManagement.Api.Features.MarketData.Yahoo;
using PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;
using PortfolioManagement.Api.Features.Portfolios.DeletePortfolio;
using PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfolio;
using PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfolios;
using PortfolioManagement.Api.Features.StockQuotes.GetStockQuote;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory;
using PortfolioManagement.Api.Features.StockHistory.GetStockHistory.Proxy;
using PortfolioManagement.Api.Features.StockProfiles.GetStockProfile;
using PortfolioManagement.Api.Features.StockProfiles.GetStockProfile.Proxy;
using PortfolioManagement.Api.Features.Trades.AddTrade;
using PortfolioManagement.Api.Features.Trades.DeleteTrade;
using PortfolioManagement.Api.Features.Trades.EditTrade;
using PortfolioManagement.Api.Infrastructure.Auth;
using PortfolioManagement.Api.Shared.Events;
using System.Net.Http.Headers;
using PortfolioManagement.Api.Features.Portfolios.Queries.GetPortfoliosOverview;

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
        services.AddMemoryCache();

        services
            .AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<PortfolioDbContext>()
            .AddDefaultTokenProviders();

        // Add-Migration InitialMigration -Context PortfolioDbContext -OutputDir Infrastructure/Migrations
        // Update-Database -Context PortfolioDbContext

        // Portfolio
        services.AddScoped<CreatePortfolioHandler>();
        services.AddScoped<IValidator<CreatePortfolioRequest>, CreatePortfolioValidator>();
        services.AddScoped<DeletePortfolioHandler>();
        services.AddScoped<GetPortfolioQuery>();
        services.AddScoped<GetPortfoliosQuery>();
        services.AddScoped<GetPortfoliosOverviewQuery>();

        // Trade
        services.AddScoped<AddTradeHandler>();
        services.AddScoped<IValidator<AddTradeRequest>, AddTradeValidator>();
        services.AddScoped<DeleteTradeHandler>();
        services.AddScoped<EditTradeHandler>();
        services.AddScoped<IValidator<EditTradeRequest>, EditTradeValidator>();

        // Instruments
        services.AddScoped<SearchInstrumentsHandler>();
        services.AddScoped<MassiveSearchProxy>();
        services.AddScoped<FinnhubSearchProxy>();
        services.AddScoped<FinnhubQuoteProxy>();
        services.AddScoped<FinnhubProfileProxy>();
        services.AddSingleton<MarketDataProviderRouter>();
        services.AddSingleton<YahooRequestGate>();
        services.AddScoped<YahooMarketDataProxy>();
        services.AddScoped<IValidator<SearchInstrumentsRequest>, SearchInstrumentsValidator>();
        //services.AddScoped<IDomainEventHandler<TradeAddedEvent>, TrackInstrumentWhenTradeAddedHandler>();

        // Stock Quotes
        services.AddScoped<GetStockQuoteHandler>();
        services.AddScoped<IValidator<GetStockQuoteRequest>, GetStockQuoteValidator>();

        // Stock History
        services.AddScoped<GetStockHistoryHandler>();
        services.AddScoped<MassiveStockHistoryProxy>();
        services.AddScoped<IValidator<GetStockHistoryRequest>, GetStockHistoryValidator>();

        // Stock Profile
        services.AddScoped<GetStockProfileHandler>();
        services.AddScoped<MassiveProfileProxy>();
        services.AddScoped<IValidator<GetStockProfileRequest>, GetStockProfileValidator>();
        services.AddHttpClient("Finnhub", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["Finnhub:BaseUrl"] ?? "https://finnhub.io");
        });

        services.AddHttpClient("Massive", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["Massive:BaseUrl"] ?? "https://api.massive.com");

            var apiKey = config["Massive:ApiKey"];

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
        });

        // Auth
        services.AddScoped<LoginHandler>();
        services.AddScoped<MeHandler>();
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();

        // Events
        services.AddScoped<DomainEventDispatcher>();

        return services;
    }
}
