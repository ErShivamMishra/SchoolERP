using Microsoft.AspNetCore.Http;

namespace SchoolERP.API.Common.Requests;

public sealed class UploadStudentDocumentFormRequestDto
{
    public string Title { get; init; } = string.Empty;
    public IFormFile? File { get; init; }
}

public sealed class UploadStudyMaterialFormRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TeacherId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IFormFile? File { get; init; }
}

public sealed class CreateHomeworkAssignmentFormRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TeacherId { get; init; }
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public IFormFile? Attachment { get; init; }
}

public sealed class CreateNoticeFormRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int NoticeType { get; init; }
    public int AudienceType { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public IFormFile? Attachment { get; init; }
}

public sealed class SendConversationMessageFormRequestDto
{
    public string MessageText { get; init; } = string.Empty;
    public int SenderType { get; init; }
    public IFormFile? Attachment { get; init; }
}

public sealed class UploadGalleryMediaFormRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MediaType { get; init; }
    public IFormFile? File { get; init; }
}
