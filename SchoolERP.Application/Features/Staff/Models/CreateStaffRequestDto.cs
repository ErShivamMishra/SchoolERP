namespace SchoolERP.Application.Features.Staff.Models;

public sealed class CreateStaffRequestDto
{
    public Guid? SchoolId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
}
