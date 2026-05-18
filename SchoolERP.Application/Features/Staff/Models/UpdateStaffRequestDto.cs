namespace SchoolERP.Application.Features.Staff.Models;

public sealed class UpdateStaffRequestDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
}
