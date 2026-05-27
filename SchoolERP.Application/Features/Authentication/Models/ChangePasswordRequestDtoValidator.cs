using FluentValidation;

namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.NewPassword)
            .Must((request, value) => !string.Equals(request.CurrentPassword, value, StringComparison.Ordinal))
            .WithMessage("New password must be different from the current password.");
    }
}
