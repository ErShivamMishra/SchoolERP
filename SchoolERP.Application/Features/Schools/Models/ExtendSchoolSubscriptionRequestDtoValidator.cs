using FluentValidation;

namespace SchoolERP.Application.Features.Schools.Models;

public sealed class ExtendSchoolSubscriptionRequestDtoValidator : AbstractValidator<ExtendSchoolSubscriptionRequestDto>
{
    public ExtendSchoolSubscriptionRequestDtoValidator()
    {
        RuleFor(x => x.NewSubscriptionEndDate).NotEmpty();
    }
}
