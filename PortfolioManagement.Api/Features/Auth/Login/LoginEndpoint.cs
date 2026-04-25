using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PortfolioManagement.Api.Infrastructure.Auth;

namespace PortfolioManagement.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this WebApplication app)
    {
        app.MapPost("/auth/login", async (
            LoginRequest request, 
            LoginHandler loginHandler,
            IValidator<LoginRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var result = await loginHandler.Handle(request);

                return result is null
                    ? Results.Unauthorized()
                    : Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError("Server is unreachable at the moment.");
            }

        });
    }

}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string FirstName, string LastName);


public class LoginHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public LoginHandler(UserManager<AppUser> userManager, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> Handle(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email); // Handle expection (server offline etc)

        if (user is null) 
            return null;

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
            return null;

        var token = _jwtTokenService.CreateToken(user);

        return new LoginResponse(
            token,
            user.Email!,
            user.FirstName,
            user.LastName
        );
    }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}