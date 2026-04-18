using PortfolioManagement.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;

namespace PortfolioManagement.Api.Features.Auth.Register;

public class RegisterHandler
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email) != null;
        if (userExists)
        {
            throw new Exception("User with this email already exists.");
        }

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration failed: {errors}");
        }

        return new RegisterResponse(user.FirstName, user.LastName, user.Email, user.UserName);
    }
}

