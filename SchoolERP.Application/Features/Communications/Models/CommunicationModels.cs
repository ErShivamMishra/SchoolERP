using FluentValidation;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Communications.Models;

public sealed class CreateConversationRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public Guid TeacherId { get; init; }
    public string Subject { get; init; } = string.Empty;
}

public sealed class SendMessageRequestDto
{
    public string MessageText { get; init; } = string.Empty;
    public MessageSenderType SenderType { get; init; }
    public FileUploadPayload? Attachment { get; init; }
}

public sealed class ConversationDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid TeacherId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public DateTime LastMessageAt { get; init; }
    public bool IsClosed { get; init; }
}

public sealed class ConversationMessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderUserId { get; init; }
    public MessageSenderType SenderType { get; init; }
    public string MessageText { get; init; } = string.Empty;
    public string? AttachmentUrl { get; init; }
    public string? OriginalFileName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetConversationListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? TeacherId { get; init; }
    public Guid? StudentId { get; init; }
}

public sealed class CreateConversationRequestDtoValidator : AbstractValidator<CreateConversationRequestDto>
{
    public CreateConversationRequestDtoValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(150);
    }
}

public sealed class SendMessageRequestDtoValidator : AbstractValidator<SendMessageRequestDto>
{
    public SendMessageRequestDtoValidator()
    {
        RuleFor(x => x.MessageText).NotEmpty().MaximumLength(4000);
    }
}

public sealed class GetConversationListRequestDtoValidator : SearchablePagedRequestValidator<GetConversationListRequestDto>
{
}
