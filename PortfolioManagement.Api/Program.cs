using Microsoft.AspNetCore.Identity;
using PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;
using PortfolioManagement.Api.Infrastructure.Auth;
using PortfolioManagement.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddAuthInfrastructure(builder.Configuration);
builder.Services.AddPortfolioInfrastructure(builder.Configuration);

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

//    var email = "test@test.com";
//    var existing = await userManager.FindByEmailAsync(email);

//    if (existing is null)
//    {
//        var user = new AppUser
//        {
//            FirstName = "Bilal",
//            LastName = "Kinali",
//            UserName = "botjefferson",
//            Email = email,
//            EmailConfirmed = true
//        };

//        await userManager.CreateAsync(user, "Test123!");
//    }
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCreatePortfolioEndpoints();

app.Run();