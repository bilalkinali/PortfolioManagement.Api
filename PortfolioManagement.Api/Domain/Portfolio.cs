using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Domain;

public class Portfolio
{
    protected Portfolio() { }

    private readonly List<Position> _positions = [];

    private Portfolio(string name, string? description, string userId)
    {
        Name = name;
        Description = description;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public int Id { get; protected set; }
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string UserId { get; protected set; } = null!;
    public AppUser User { get; protected set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<Position> Positions => _positions.AsReadOnly();

    public static Portfolio Create(string name, string? description, string userId)
    {
        return new Portfolio(name, description, userId);
    }

    public Trade AddTrade(int instrumentId, int quantity, decimal price, DateOnly executedDate)
    {
        var position = _positions.FirstOrDefault(p => p.InstrumentId == instrumentId);

        if (position is not null)
        {
            return position.AddTrade(quantity, price, executedDate);
        }

        position = Position.Create(instrumentId, quantity, price, executedDate, out var trade);
        _positions.Add(position);

        return trade;
    }

    public void EditTrade(int positionId, int tradeId, int quantity, decimal price, DateOnly executedDate)
    {
        var position = GetPosition(positionId);

        position.EditTrade(tradeId, quantity, price, executedDate);
    }

    public void DeleteTrade(int positionId, int tradeId)
    {
        var position = GetPosition(positionId);

        position.DeleteTrade(tradeId);

        if (!position.HasTrades)
        {
            _positions.Remove(position);
        }
    }

    private Position GetPosition(int positionId)
    {
        return _positions.FirstOrDefault(p => p.Id == positionId)
            ?? throw new InvalidOperationException("Position does not exist.");
    }
}