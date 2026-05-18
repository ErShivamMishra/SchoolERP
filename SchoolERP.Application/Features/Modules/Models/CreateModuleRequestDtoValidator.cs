using FluentValidation;

namespace SchoolERP.Application.Features.Modules.Models;

public sealed class CreateModuleRequestDtoValidator : AbstractValidator<CreateModuleRequestDto>
{
    public CreateModuleRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Za-z0-9]+$").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
