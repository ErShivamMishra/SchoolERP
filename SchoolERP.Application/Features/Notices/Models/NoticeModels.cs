using FluentValidation;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Notices.Models;

public sealed class CreateNoticeRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public NoticeType NoticeType { get; init; }
    public NoticeAudienceType AudienceType { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public FileUploadPayload? Attachment { get; init; }
}

public sealed class PublishNoticeRequestDto
{
    public bool IsPublished { get; init; }
}

public sealed class NoticeDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public NoticeType NoticeType { get; init; }
    public NoticeAudienceType AudienceType { get; init; }
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? SectionId { get; init; }
    public string? SectionName { get; init; }
    public bool IsPublished { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? OriginalFileName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetNoticeListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public NoticeType? NoticeType { get; init; }
    public NoticeAudienceType? AudienceType { get; init; }
    public bool? IsPublished { get; init; }
}

public sealed class CreateNoticeRequestDtoValidator : AbstractValidator<CreateNoticeRequestDto>
{
    public CreateNoticeRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public sealed class GetNoticeListRequestDtoValidator : SearchablePagedRequestValidator<GetNoticeListRequestDto>
{
}
