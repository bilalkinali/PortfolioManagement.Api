using PortfolioManagement.Api.Domain;

namespace PortfolioManagement.Api.Features.Portfolios.CreatePortfolio;

public class CreatePortfolioHandler
{
    public static Task<CreatePortfolioResponse> Handle(CreatePortfolioRequest request, string userId)
    {
        // Perform validation, check for existing portfolios, and save the portfolio to a database.
        var portfolio = Portfolio.Create(request.Name, request.Description, userId);

        Console.WriteLine("Created Portfolio, saving to database... (missing)");
        
        // Simulate saving to a database and getting an ID back
        portfolio.GetType().GetProperty("Id")?.SetValue(portfolio, new Random().Next(1, 1000));
        
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
        

        return Task.FromResult(response);
    }
}