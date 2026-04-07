using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Domain;

public class Portfolio
{
    protected Portfolio() {}

    private Portfolio(string name, string? description, string userId)
    {
        Name = name;
        Description = description;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private readonly List<Position> _positions = [];
    
    public int Id { get; protected set; }
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string UserId { get; protected set; } = null!;
    public AppUser User { get; protected set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<Position> Positions => _positions.AsReadOnly();


    /**************************************************************************************/
    

    public static Portfolio Create(string name, string? description, string userId)
    {
        return new Portfolio(name, description, userId);
    }

    // Trades
    public Trade AddTrade(string symbol, int quantity, decimal price, DateOnly executedDate)
    {
        // input validation needed (Also handle symbol normalization, e.g. AAPL vs aapl)
        var position = _positions.FirstOrDefault(p => p.Symbol == symbol);

        if (position is null)
        {
            position = AddPosition(symbol);
        }

        return position.AddTrade(quantity, price, executedDate);
    }


    // Positions
    private Position AddPosition(string symbol)
    {
        var position = Position.Create(symbol);
        _positions.Add(position);
        return position;
    }

}