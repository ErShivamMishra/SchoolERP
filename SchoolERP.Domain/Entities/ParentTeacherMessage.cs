using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class ParentTeacherMessage : AuditableEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public MessageSenderType SenderType { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }

    public ParentTeacherConversation? Conversation { get; set; }
    public User? SenderUser { get; set; }
}
