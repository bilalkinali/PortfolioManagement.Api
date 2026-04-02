namespace PortfolioManagement.Api.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request) =>
        {
            var response = await RegisterHandler.Handle(request);
            return Results.Ok(response); // Results.Created("location", response)?
        });
    }
}