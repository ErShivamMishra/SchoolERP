using FluentValidation;
using SchoolERP.Application.Common.Models;

namespace SchoolERP.Application.Features.AuditLogs.Models;

public sealed class AuditLogListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? UserId { get; init; }
    public string? Module { get; init; }
    public string? Action { get; init; }
}

public sealed class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid? SchoolId { get; init; }
    public Guid? UserId { get; init; }
    public string Module { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? Details { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class AuditLogListRequestDtoValidator : SearchablePagedRequestValidator<AuditLogListRequestDto>
{
}
