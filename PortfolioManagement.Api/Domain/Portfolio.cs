namespace PortfolioManagement.Api.Domain;

public class Portfolio
{
    protected Portfolio() {}

    private Portfolio(string name, string? description)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private readonly List<Position> _positions = [];
    
    public int Id { get; protected set; }
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<Position> Positions => _positions;


    /**************************************************************************************/
    

    public static Portfolio Create(string name, string? description)
    {
        return new Portfolio(name.Trim(), description?.Trim());
    }


    // Positions
    public Position AddPosition(int id)
    {
        var position = Position.Create(id);
        _positions.Add(position);
        return position;
    }

}