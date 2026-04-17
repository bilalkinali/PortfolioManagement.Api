using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PortfolioManagement.Api.Features.Auth.Login;
using PortfolioManagement.Api.Features.Auth.Register;
using PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;
using PortfolioManagement.Api.Features.Trades.AddTrade;
using PortfolioManagement.Api.Infrastructure.Persistence;
using System.Text;
using PortfolioManagement.Api.Features.Auth.Me;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddAuthInfrastructure(builder.Configuration);
builder.Services.AddPortfolioInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapLoginEndpoints();
app.MapMeEndpoints();
app.MapRegisterEndpoints();
app.MapCreatePortfolioEndpoints();
app.MapAddTradeEndpoints();

app.Run();