using PortfolioManagement.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace PortfolioManagement.Api.Infrastructure.Auth;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}