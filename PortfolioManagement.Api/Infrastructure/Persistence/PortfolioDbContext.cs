using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Infrastructure.Persistence;

public class PortfolioDbContext : IdentityDbContext<AppUser>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Instrument> Instruments { get; set; }
    public DbSet<MarketDataBar> MarketDataBars { get; set; }

    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-10.0

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.Email).IsRequired();
        });

        builder.Entity<Instrument>(entity =>
        {
            entity.HasIndex(x => x.Symbol)
                .IsUnique();
        });

        builder.Entity<MarketDataBar>(entity =>
        {
            entity.HasIndex(x => new { x.InstrumentId, x.Date })
                .IsUnique();
        });

        //builder.Entity<Portfolio>(entity =>
        //{
        //    entity.HasKey(x => x.Id);

        //    entity.HasOne(x => x.User)
        //        .WithMany(x => x.Portfolios)
        //        .HasForeignKey(x => x.UserId)
        //        .IsRequired();

        //    entity.Navigation(x => x.Positions)
        //        .UsePropertyAccessMode(PropertyAccessMode.Field);
        //});
    }
}
