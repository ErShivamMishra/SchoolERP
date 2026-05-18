using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Models;

public sealed class UpdateStaffRequestDtoValidator : AbstractValidator<UpdateStaffRequestDto>
{
    public UpdateStaffRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
