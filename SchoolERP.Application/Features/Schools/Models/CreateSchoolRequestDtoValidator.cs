using FluentValidation;

namespace SchoolERP.Application.Features.Schools.Models;

public sealed class CreateSchoolRequestDtoValidator : AbstractValidator<CreateSchoolRequestDto>
{
    public CreateSchoolRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.MaxStaffLimit).GreaterThan(0);
        RuleFor(x => x.SubscriptionStartDate).NotEmpty();
        RuleFor(x => x.SubscriptionEndDate)
            .GreaterThan(x => x.SubscriptionStartDate)
            .WithMessage("Subscription end date must be after the start date.");
    }
}
