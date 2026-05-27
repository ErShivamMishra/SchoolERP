using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Communications.Interfaces;
using SchoolERP.Application.Features.Communications.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Communications.Services;

public sealed class CommunicationService(
    ICommunicationRepository repository,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateConversationRequestDto> createValidator,
    IValidator<SendMessageRequestDto> sendValidator,
    IValidator<GetConversationListRequestDto> listValidator) : ICommunicationService
{
    private static readonly string[] AllowedAttachmentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public async Task<ConversationDto> CreateConversationAsync(CreateConversationRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var student = await repository.GetStudentByIdAsync(request.StudentId, cancellationToken) ?? throw new NotFoundException("Student not found.", "student_not_found");
        var teacher = await repository.GetTeacherByIdAsync(request.TeacherId, cancellationToken) ?? throw new NotFoundException("Teacher not found.", "teacher_not_found");
        EnsureSchoolOwned(schoolId, student.SchoolId);
        EnsureSchoolOwned(schoolId, teacher.SchoolId);

        var existing = await repository.GetConversationAsync(schoolId, request.StudentId, request.TeacherId, request.Subject.Trim(), cancellationToken);
        if (existing is not null)
        {
            return MapConversation(existing);
        }

        var conversation = new ParentTeacherConversation
        {
            SchoolId = schoolId,
            StudentId = request.StudentId,
            TeacherId = request.TeacherId,
            Subject = request.Subject.Trim(),
            LastMessageAtUtc = DateTime.UtcNow,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddConversationAsync(conversation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetConversationByIdAsync(conversation.Id, cancellationToken) ?? conversation;
        await auditService.WriteAsync(ModuleCodes.CommunicationManagement, "ConversationCreated", nameof(ParentTeacherConversation), conversation.Id.ToString(), "Success", "Conversation created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapConversation(saved);
    }

    public async Task<ConversationMessageDto> SendMessageAsync(Guid conversationId, SendMessageRequestDto request, CancellationToken cancellationToken)
    {
        await sendValidator.ValidateAndThrowAsync(request, cancellationToken);
        var conversation = await GetManagedConversationAsync(conversationId, cancellationToken);
        if (!currentUserContext.UserId.HasValue)
        {
            throw new UnauthorizedException("Authentication is required.", "authentication_required");
        }

        FileUploadResult? upload = null;
        if (request.Attachment is not null)
        {
            FileUploadValidationHelper.Validate(request.Attachment, AllowedAttachmentTypes, 5 * 1024 * 1024, "Conversation attachment");
            upload = await fileStorageService.UploadAsync(conversation.SchoolId, ModuleCodes.CommunicationManagement, "messages", request.Attachment, cancellationToken);
        }

        var message = new ParentTeacherMessage
        {
            ConversationId = conversation.Id,
            SenderUserId = currentUserContext.UserId.Value,
            SenderType = request.SenderType,
            MessageText = request.MessageText.Trim(),
            AttachmentUrl = upload?.FileUrl,
            OriginalFileName = upload?.OriginalFileName,
            StoredFileName = upload?.StoredFileName,
            ContentType = upload?.ContentType,
            FileSize = upload?.FileSize,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        conversation.LastMessageAtUtc = DateTime.UtcNow;
        conversation.ModifiedAtUtc = DateTime.UtcNow;
        conversation.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.AddMessageAsync(message, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.CommunicationManagement, "ConversationMessageSent", nameof(ParentTeacherMessage), message.Id.ToString(), "Success", "Conversation message sent.", conversation.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapMessage(message);
    }

    public async Task<PagedResult<ConversationDto>> GetConversationsAsync(GetConversationListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetConversationPageAsync(schoolId, request.TeacherId, request.StudentId, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<ConversationDto>
        {
            Items = items.Select(MapConversation).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyCollection<ConversationMessageDto>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await GetManagedConversationAsync(conversationId, cancellationToken);
        return (await repository.GetMessagesAsync(conversation.Id, cancellationToken)).Select(MapMessage).ToArray();
    }

    private async Task<ParentTeacherConversation> GetManagedConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationByIdAsync(conversationId, cancellationToken) ?? throw new NotFoundException("Conversation not found.", "conversation_not_found");
        EnsureSchoolOwned(ResolveSchoolIdForRead(conversation.SchoolId), conversation.SchoolId);
        return conversation;
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
            throw new ForbiddenException("Communication access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Communication access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private static void EnsureSchoolOwned(Guid expected, Guid actual)
    {
        if (expected != actual)
        {
            throw new ForbiddenException("Communication access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static ConversationDto MapConversation(ParentTeacherConversation conversation) => new()
    {
        Id = conversation.Id,
        SchoolId = conversation.SchoolId,
        StudentId = conversation.StudentId,
        StudentName = conversation.Student is null ? string.Empty : $"{conversation.Student.FirstName} {conversation.Student.LastName}".Trim(),
        TeacherId = conversation.TeacherId,
        TeacherName = conversation.Teacher is null ? string.Empty : $"{conversation.Teacher.FirstName} {conversation.Teacher.LastName}".Trim(),
        Subject = conversation.Subject,
        LastMessageAt = conversation.LastMessageAtUtc,
        IsClosed = conversation.IsClosed
    };

    private static ConversationMessageDto MapMessage(ParentTeacherMessage message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        SenderUserId = message.SenderUserId,
        SenderType = message.SenderType,
        MessageText = message.MessageText,
        AttachmentUrl = message.AttachmentUrl,
        OriginalFileName = message.OriginalFileName,
        CreatedAt = message.CreatedAtUtc
    };
}
