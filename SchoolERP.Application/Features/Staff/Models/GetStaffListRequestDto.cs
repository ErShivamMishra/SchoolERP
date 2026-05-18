namespace SchoolERP.Application.Features.Staff.Models;

public sealed class GetStaffListRequestDto
{
    public Guid? SchoolId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public Guid? RoleId { get; init; }
    public bool? IsActive { get; init; }
}
