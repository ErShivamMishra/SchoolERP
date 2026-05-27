using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Notices.Interfaces;
using SchoolERP.Application.Features.Notices.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Notices.Services;

public sealed class NoticeService(
    INoticeRepository repository,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateNoticeRequestDto> createValidator,
    IValidator<GetNoticeListRequestDto> listValidator) : INoticeService
{
    private static readonly string[] AllowedAttachmentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public async Task<NoticeDto> CreateAsync(CreateNoticeRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureAudienceScopeAsync(schoolId, request.ClassId, request.SectionId, cancellationToken);

        FileUploadResult? upload = null;
        if (request.Attachment is not null)
        {
            FileUploadValidationHelper.Validate(request.Attachment, AllowedAttachmentTypes, 5 * 1024 * 1024, "Notice attachment");
            upload = await fileStorageService.UploadAsync(schoolId, ModuleCodes.NoticeBoardManagement, "attachments", request.Attachment, cancellationToken);
        }

        var notice = new NoticeBoardItem
        {
            SchoolId = schoolId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            NoticeType = request.NoticeType,
            AudienceType = request.AudienceType,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            ExpiryDateUtc = request.ExpiryDate,
            AttachmentUrl = upload?.FileUrl,
            OriginalFileName = upload?.OriginalFileName,
            StoredFileName = upload?.StoredFileName,
            ContentType = upload?.ContentType,
            FileSize = upload?.FileSize,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddAsync(notice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetByIdAsync(notice.Id, cancellationToken) ?? notice;
        await auditService.WriteAsync(ModuleCodes.NoticeBoardManagement, "NoticeCreated", nameof(NoticeBoardItem), notice.Id.ToString(), "Success", "Notice created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapNotice(saved);
    }

    public async Task<NoticeDto> PublishAsync(Guid noticeId, PublishNoticeRequestDto request, CancellationToken cancellationToken)
    {
        var notice = await GetManagedNoticeAsync(noticeId, cancellationToken);
        notice.IsPublished = request.IsPublished;
        notice.PublishedAtUtc = request.IsPublished ? DateTime.UtcNow : null;
        notice.ModifiedAtUtc = DateTime.UtcNow;
        notice.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.NoticeBoardManagement, "NoticePublishUpdated", nameof(NoticeBoardItem), notice.Id.ToString(), "Success", request.IsPublished ? "Notice published." : "Notice unpublished.", notice.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapNotice(notice);
    }

    public async Task<PagedResult<NoticeDto>> GetAllAsync(GetNoticeListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetPageAsync(schoolId, request.NoticeType, request.AudienceType, request.IsPublished, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<NoticeDto>
        {
            Items = items.Select(MapNotice).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<NoticeDto> GetByIdAsync(Guid noticeId, CancellationToken cancellationToken)
        => MapNotice(await GetManagedNoticeAsync(noticeId, cancellationToken));

    private async Task<NoticeBoardItem> GetManagedNoticeAsync(Guid noticeId, CancellationToken cancellationToken)
    {
        var notice = await repository.GetByIdAsync(noticeId, cancellationToken) ?? throw new NotFoundException("Notice not found.", "notice_not_found");
        EnsureSchoolAccess(notice.SchoolId);
        return notice;
    }

    private async Task EnsureAudienceScopeAsync(Guid schoolId, Guid? classId, Guid? sectionId, CancellationToken cancellationToken)
    {
        if (classId.HasValue)
        {
            var classEntity = await repository.GetClassByIdAsync(classId.Value, cancellationToken) ?? throw new NotFoundException("Class not found.", "class_not_found");
            if (classEntity.TenantId != schoolId)
            {
                throw new ForbiddenException("Notice access is limited to the current school.", "cross_tenant_access_forbidden");
            }
        }

        if (sectionId.HasValue)
        {
            var section = await repository.GetSectionByIdAsync(sectionId.Value, cancellationToken) ?? throw new NotFoundException("Section not found.", "section_not_found");
            if (section.TenantId != schoolId || (classId.HasValue && section.ClassId != classId.Value))
            {
                throw new ForbiddenException("Notice access is limited to the current school.", "cross_tenant_access_forbidden");
            }
        }
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            var schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            return schoolId;
        }
        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }
        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Notice access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }
        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }
        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Notice access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Notice access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static NoticeDto MapNotice(NoticeBoardItem notice) => new()
    {
        Id = notice.Id,
        SchoolId = notice.SchoolId,
        Title = notice.Title,
        Content = notice.Content,
        NoticeType = notice.NoticeType,
        AudienceType = notice.AudienceType,
        ClassId = notice.ClassId,
        ClassName = notice.Class?.Name,
        SectionId = notice.SectionId,
        SectionName = notice.Section?.Name,
        IsPublished = notice.IsPublished,
        PublishedAt = notice.PublishedAtUtc,
        ExpiryDate = notice.ExpiryDateUtc,
        AttachmentUrl = notice.AttachmentUrl,
        OriginalFileName = notice.OriginalFileName,
        CreatedAt = notice.CreatedAtUtc
    };
}
