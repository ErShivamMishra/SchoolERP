using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Gallery.Interfaces;
using SchoolERP.Application.Features.Gallery.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Gallery.Services;

public sealed class GalleryService(
    IGalleryRepository repository,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateAlbumRequestDto> createValidator,
    IValidator<UploadGalleryMediaRequestDto> uploadValidator,
    IValidator<GetAlbumListRequestDto> listValidator) : IGalleryService
{
    private static readonly string[] AllowedMediaTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "video/mp4",
        "video/webm"
    ];

    public async Task<GalleryAlbumDto> CreateAlbumAsync(CreateAlbumRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var album = new GalleryAlbum
        {
            SchoolId = schoolId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddAlbumAsync(album, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.GalleryManagement, "AlbumCreated", nameof(GalleryAlbum), album.Id.ToString(), "Success", "Gallery album created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapAlbum(album);
    }

    public async Task<GalleryAlbumDto> PublishAlbumAsync(Guid albumId, PublishAlbumRequestDto request, CancellationToken cancellationToken)
    {
        var album = await GetManagedAlbumAsync(albumId, cancellationToken);
        album.IsPublished = request.IsPublished;
        album.PublishedAtUtc = request.IsPublished ? DateTime.UtcNow : null;
        album.ModifiedAtUtc = DateTime.UtcNow;
        album.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.GalleryManagement, "AlbumPublishUpdated", nameof(GalleryAlbum), album.Id.ToString(), "Success", request.IsPublished ? "Album published." : "Album unpublished.", album.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapAlbum(album);
    }

    public async Task<GalleryMediaDto> UploadMediaAsync(Guid albumId, UploadGalleryMediaRequestDto request, CancellationToken cancellationToken)
    {
        await uploadValidator.ValidateAndThrowAsync(request, cancellationToken);
        var album = await GetManagedAlbumAsync(albumId, cancellationToken);
        FileUploadValidationHelper.Validate(request.File, AllowedMediaTypes, 50 * 1024 * 1024, "Gallery media");
        var upload = await fileStorageService.UploadAsync(album.SchoolId, ModuleCodes.GalleryManagement, "media", request.File!, cancellationToken);
        var media = new GalleryMedia
        {
            SchoolId = album.SchoolId,
            AlbumId = albumId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            MediaType = request.MediaType,
            MediaUrl = upload.FileUrl,
            OriginalFileName = upload.OriginalFileName,
            StoredFileName = upload.StoredFileName,
            ContentType = upload.ContentType,
            FileSize = upload.FileSize,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddMediaAsync(media, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.GalleryManagement, "MediaUploaded", nameof(GalleryMedia), media.Id.ToString(), "Success", "Gallery media uploaded.", album.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapMedia(media);
    }

    public async Task<PagedResult<GalleryAlbumDto>> GetAlbumsAsync(GetAlbumListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetAlbumPageAsync(schoolId, request.IsPublished, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<GalleryAlbumDto>
        {
            Items = items.Select(MapAlbum).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyCollection<GalleryMediaDto>> GetMediaAsync(Guid albumId, CancellationToken cancellationToken)
    {
        var album = await GetManagedAlbumAsync(albumId, cancellationToken);
        return (await repository.GetMediaAsync(album.Id, cancellationToken)).Select(MapMedia).ToArray();
    }

    private async Task<GalleryAlbum> GetManagedAlbumAsync(Guid albumId, CancellationToken cancellationToken)
    {
        var album = await repository.GetAlbumByIdAsync(albumId, cancellationToken) ?? throw new NotFoundException("Album not found.", "album_not_found");
        EnsureSchoolAccess(album.SchoolId);
        return album;
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
            throw new ForbiddenException("Gallery access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Gallery access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Gallery access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static GalleryAlbumDto MapAlbum(GalleryAlbum album) => new()
    {
        Id = album.Id,
        SchoolId = album.SchoolId,
        Title = album.Title,
        Description = album.Description,
        IsPublished = album.IsPublished,
        PublishedAt = album.PublishedAtUtc,
        MediaCount = album.MediaItems.Count
    };

    private static GalleryMediaDto MapMedia(GalleryMedia media) => new()
    {
        Id = media.Id,
        AlbumId = media.AlbumId,
        Title = media.Title,
        Description = media.Description,
        MediaType = media.MediaType,
        MediaUrl = media.MediaUrl,
        OriginalFileName = media.OriginalFileName,
        ContentType = media.ContentType,
        FileSize = media.FileSize,
        CreatedAt = media.CreatedAtUtc
    };
}
