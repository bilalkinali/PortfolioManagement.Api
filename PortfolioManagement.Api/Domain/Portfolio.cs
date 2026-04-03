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
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<Position> Positions => _positions;


    /**************************************************************************************/
    

    public static Portfolio Create(string name, string? description, string userId)
    {
        return new Portfolio(name, description, userId);
    }


    // Positions
    public Position AddPosition(string symbol)
    {
        var position = Position.Create(symbol);
        _positions.Add(position);
        return position;
    }

}