using AirFlights.Domain;
using FluentValidation;

namespace AirFlights.Infrastructure;

public class AuthModelValidator : AbstractValidator<AuthModel> 
{
    public AuthModelValidator()
    {
        RuleFor(authModel => authModel.Username).NotEmpty().WithMessage("UserName can't be empty");
        RuleFor(authModel => authModel.Username).NotNull().WithMessage("Password can't be null");
        RuleFor(authModel => authModel.Username).MaximumLength(256).WithMessage("UserName's length more than 256 characters");
        RuleFor(authModel => authModel.Password).NotEmpty().WithMessage("Password can't be empty");
        RuleFor(authModel => authModel.Password).NotNull().WithMessage("Password can't be null");
        RuleFor(authModel => authModel.Password).MaximumLength(256).WithMessage("Password's length more than 256 characters");
    }
}