using FluentValidation;

namespace SchoolERP.Application.Features.Schools.Models;

public sealed class SetSchoolActivationRequestDtoValidator : AbstractValidator<SetSchoolActivationRequestDto>
{
    public SetSchoolActivationRequestDtoValidator()
    {
    }
}
