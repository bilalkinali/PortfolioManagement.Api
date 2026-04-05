using PortfolioManagement.Api.Domain;
using PortfolioManagement.Api.Infrastructure.Persistence;

namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public class CreatePortfolioHandler(PortfolioDbContext db)
{
    public async Task<CreatePortfolioResponse> Handle(CreatePortfolioRequest request, string userId)
    {
        // Perform validation, check for existing portfolios, and save the portfolio to a database.
        var portfolio = Portfolio.Create(request.Name, request.Description, userId);

        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();
        
        var response = new CreatePortfolioResponse(
            portfolio.Id, 
            portfolio.Name, 
            portfolio.Description, 
            portfolio.CreatedAt,
            portfolio.UserId);
        
        // Log the properties of the CreatePortfolioResponse for debugging
        Console.WriteLine("CreatePortfolioResponse properties:");
        foreach (var prop in response.GetType().GetProperties())
        {
            Console.WriteLine($"{prop.Name}: {prop.GetValue(response)}");
        }
        

        return response;
    }

}