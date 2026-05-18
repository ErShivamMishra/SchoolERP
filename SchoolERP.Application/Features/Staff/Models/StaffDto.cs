namespace SchoolERP.Application.Features.Staff.Models;

public sealed class StaffDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsFirstLogin { get; init; }
    public DateTime CreatedAt { get; init; }
}
