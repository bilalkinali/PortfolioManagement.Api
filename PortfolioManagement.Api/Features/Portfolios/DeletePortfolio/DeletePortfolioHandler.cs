using Microsoft.EntityFrameworkCore;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.DeletePortfolio;

public class DeletePortfolioHandler(PortfolioDbContext db)
{
    public async Task Handle(int id, string userId, CancellationToken cancellationToken)
    {
        var portfolio = await db.Portfolios
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

        if (portfolio == null)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }
        
        db.Portfolios.Remove(portfolio);
        await db.SaveChangesAsync(cancellationToken);
    }
}