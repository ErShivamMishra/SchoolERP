using FluentValidation;

namespace SchoolERP.Application.Features.AccessControl.Models;

public sealed class UpsertUserPermissionsRequestDtoValidator : AbstractValidator<UpsertUserPermissionsRequestDto>
{
    public UpsertUserPermissionsRequestDtoValidator()
    {
        RuleFor(x => x.Permissions).NotNull();
        RuleForEach(x => x.Permissions).ChildRules(permission =>
        {
            permission.RuleFor(x => x.ModuleId).NotEmpty();
        });
    }
}
