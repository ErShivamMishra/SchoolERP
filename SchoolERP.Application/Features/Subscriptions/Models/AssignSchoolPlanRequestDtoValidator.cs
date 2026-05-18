using FluentValidation;

namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class AssignSchoolPlanRequestDtoValidator : AbstractValidator<AssignSchoolPlanRequestDto>
{
    public AssignSchoolPlanRequestDtoValidator()
    {
        RuleFor(x => x.SubscriptionPlanId).NotEmpty();
        RuleFor(x => x.EndDate)
            .Must((request, endDate) => endDate > request.StartDate)
            .WithMessage("End date must be later than start date.");
    }
}
