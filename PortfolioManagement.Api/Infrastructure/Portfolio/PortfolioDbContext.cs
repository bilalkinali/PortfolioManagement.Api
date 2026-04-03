using PortfolioManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace PortfolioManagement.Api.Infrastructure.Portfolio;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Portfolio> Portfolios { get; set; }
}