using System.Reflection;

namespace PortfolioManagement.Api.Domain.Tests;

internal static class DomainTestIds
{
    public static void SetId<T>(T entity, int id)
    {
        typeof(T)
            .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
    }
}
