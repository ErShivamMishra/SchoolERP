namespace SchoolERP.Application.Features.Staff.Models;

public sealed class ResetStaffPasswordResultDto
{
    public Guid StaffId { get; init; }
    public string TemporaryPassword { get; init; } = string.Empty;
}
