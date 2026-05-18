using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Models;

public sealed class CreateStaffRequestDtoValidator : AbstractValidator<CreateStaffRequestDto>
{
    public CreateStaffRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
