using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AuditLogs.Interfaces;
using SchoolERP.Application.Features.AuditLogs.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.Application.Features.AuditLogs.Services;

public sealed class AuditLogService(
    IAuditLogRepository repository,
    ICurrentUserContext currentUserContext,
    IValidator<AuditLogListRequestDto> validator) : IAuditLogService
{
    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogListRequestDto request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolScopeAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetLogsPageAsync(schoolId, request.UserId, request.Module, request.Action, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(x => new AuditLogDto
            {
                Id = x.Id,
                SchoolId = x.SchoolId,
                UserId = x.UserId,
                Module = x.Module,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Outcome = x.Outcome,
                Details = x.Details,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAtUtc
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private async Task<Guid?> ResolveSchoolScopeAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (schoolId.HasValue)
            {
                _ = await repository.GetSchoolByIdAsync(schoolId.Value, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            }

            return schoolId;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (schoolId.HasValue && schoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Audit log access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }
}
