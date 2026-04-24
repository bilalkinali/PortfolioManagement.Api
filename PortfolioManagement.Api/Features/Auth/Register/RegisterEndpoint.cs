using FluentValidation;

namespace PortfolioManagement.Api.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (
            RegisterRequest request, 
            RegisterHandler registerHandler,
            IValidator<RegisterRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var response = await registerHandler.Handle(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }
        });
    }
}