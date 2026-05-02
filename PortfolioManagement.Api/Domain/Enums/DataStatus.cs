namespace PortfolioManagement.Api.Domain.Enums;

public enum DataStatus
{
    /// <summary>
    /// The instrument has only basic identity data, typically enough for search results.
    /// Example: Symbol and Name.
    /// </summary>
    Discovered = 1,

    /// <summary>
    /// The instrument has been enriched with provider metadata.
    /// Example: Exchange, Currency, Market, Type, and ProviderSymbol.
    /// </summary>
    Enriched = 2
}