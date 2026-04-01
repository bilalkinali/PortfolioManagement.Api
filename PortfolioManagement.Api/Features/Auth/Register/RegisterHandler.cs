namespace PortfolioManagement.Api.Features.Auth.Register;

public class RegisterHandler
{

    public static Task<RegisterResponse> Handle(RegisterRequest request)
    {
        // Perform validation, check for existing users, hash the password, and save the user to a database.

        var test = new RegisterResponse(
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email);

        return Task.FromResult(test);
    }


}

