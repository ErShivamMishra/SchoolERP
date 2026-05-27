using FluentValidation;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Gallery.Models;

public sealed class CreateAlbumRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class PublishAlbumRequestDto
{
    public bool IsPublished { get; init; }
}

public sealed class UploadGalleryMediaRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MediaType MediaType { get; init; }
    public FileUploadPayload? File { get; init; }
}

public sealed class GalleryAlbumDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPublished { get; init; }
    public DateTime? PublishedAt { get; init; }
    public int MediaCount { get; init; }
}

public sealed class GalleryMediaDto
{
    public Guid Id { get; init; }
    public Guid AlbumId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MediaType MediaType { get; init; }
    public string MediaUrl { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetAlbumListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public bool? IsPublished { get; init; }
}

public sealed class CreateAlbumRequestDtoValidator : AbstractValidator<CreateAlbumRequestDto>
{
    public CreateAlbumRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UploadGalleryMediaRequestDtoValidator : AbstractValidator<UploadGalleryMediaRequestDto>
{
    public UploadGalleryMediaRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class GetAlbumListRequestDtoValidator : SearchablePagedRequestValidator<GetAlbumListRequestDto>
{
}
