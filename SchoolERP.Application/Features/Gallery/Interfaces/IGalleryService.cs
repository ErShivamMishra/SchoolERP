using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Gallery.Models;

namespace SchoolERP.Application.Features.Gallery.Interfaces;

public interface IGalleryService
{
    Task<GalleryAlbumDto> CreateAlbumAsync(CreateAlbumRequestDto request, CancellationToken cancellationToken);
    Task<GalleryAlbumDto> PublishAlbumAsync(Guid albumId, PublishAlbumRequestDto request, CancellationToken cancellationToken);
    Task<GalleryMediaDto> UploadMediaAsync(Guid albumId, UploadGalleryMediaRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<GalleryAlbumDto>> GetAlbumsAsync(GetAlbumListRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GalleryMediaDto>> GetMediaAsync(Guid albumId, CancellationToken cancellationToken);
}
