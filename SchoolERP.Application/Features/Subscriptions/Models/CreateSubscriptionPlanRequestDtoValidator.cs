using FluentValidation;

namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class CreateSubscriptionPlanRequestDtoValidator : AbstractValidator<CreateSubscriptionPlanRequestDto>
{
    public CreateSubscriptionPlanRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Za-z0-9_-]+$").MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
