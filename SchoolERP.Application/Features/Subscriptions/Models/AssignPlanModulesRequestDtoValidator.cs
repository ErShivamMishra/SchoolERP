using FluentValidation;

namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class AssignPlanModulesRequestDtoValidator : AbstractValidator<AssignPlanModulesRequestDto>
{
    public AssignPlanModulesRequestDtoValidator()
    {
        RuleFor(x => x.Modules).NotNull();
        RuleForEach(x => x.Modules).ChildRules(module =>
        {
            module.RuleFor(x => x.ModuleId).NotEmpty();
        });
    }
}
