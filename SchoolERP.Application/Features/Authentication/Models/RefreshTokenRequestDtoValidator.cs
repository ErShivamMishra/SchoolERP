using FluentValidation;

namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(2048);
    }
}
