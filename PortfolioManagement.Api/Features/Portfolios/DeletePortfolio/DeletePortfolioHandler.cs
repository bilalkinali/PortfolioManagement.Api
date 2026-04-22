using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.DeletePortfolio;

public class DeletePortfolioHandler(PortfolioDbContext db)
{
    public async Task Handle(int id, string userId)
    {
        var portfolio = await db.Portfolios.FindAsync(id);

        if (portfolio == null)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }

        portfolio.AssureUserIsCreator(userId);

        db.Portfolios.Remove(portfolio);
        await db.SaveChangesAsync();
    }
}
