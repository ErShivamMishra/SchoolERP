using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Models;

public sealed class GetStaffListRequestDtoValidator : AbstractValidator<GetStaffListRequestDto>
{
    public GetStaffListRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
