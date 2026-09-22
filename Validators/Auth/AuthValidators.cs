using FluentValidation;
using BareBonesApi.DTOs.Auth;

namespace BareBonesApi.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Must be a valid email");

        RuleFor(x => x.Phone)
            .MinimumLength(10).WithMessage("Phone number must be valid")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.AgreedToTerms)
            .Equal(true).WithMessage("You must agree to the Terms & Privacy Policy");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Username or email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Must be a valid email");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password");
    }
}