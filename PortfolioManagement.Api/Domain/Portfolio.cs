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


    public int Id { get; protected set; }
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public DateTimeOffset CreatedAt { get; private set; }

    

    public static Portfolio Create(string name, string? description)
    {
        return new Portfolio(name.Trim(), description?.Trim());
    }
    
}