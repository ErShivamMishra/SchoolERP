using FluentValidation;

namespace SchoolERP.Application.Features.Schools.Models;

public sealed class UpdateSchoolRequestDtoValidator : AbstractValidator<UpdateSchoolRequestDto>
{
    public UpdateSchoolRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.MaxStaffLimit).GreaterThan(0);
    }
}
