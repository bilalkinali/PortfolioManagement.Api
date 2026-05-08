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
    public Trade AddTrade(int instrumentId, int quantity, decimal price, DateOnly executedDate)
    {
        var position = _positions.FirstOrDefault(p => p.InstrumentId == instrumentId);

        if (position is null)
        {
            position = AddPosition(instrumentId);
        }

        return position.AddTrade(quantity, price, executedDate);
    }

    public void EditTrade(int positionId, int tradeId, int quantity, decimal price, DateOnly executedDate, string userId)
    {
        AssureUserIsCreator(userId);

        var position = GetPosition(positionId);

        position.EditTrade(tradeId, quantity, price, executedDate);
    }

    public void DeleteTrade(int positionId, int tradeId, string userId)
    {
        AssureUserIsCreator(userId);

        var position = GetPosition(positionId);

        position.DeleteTrade(tradeId);

        if (!position.HasTrades)
        {
            _positions.Remove(position);
        }
    }


    public void AssureUserIsCreator(string userId)
    {
        if (!UserId.Equals(userId))
        {
            throw new ArgumentException("Only the creator of the portfolio can perform this action");
        }
    }

    private Position GetPosition(int positionId)
    {
        var position = _positions.FirstOrDefault(p => p.Id == positionId);

        if (position is null)
        {
            throw new InvalidOperationException("Position doesn't exist");
        }

        return position;
    }

    // Positions
    private Position AddPosition(int instrumentId)
    {
        var position = Position.Create(instrumentId);
        _positions.Add(position);
        return position;
    }

}
