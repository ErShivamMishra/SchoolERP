using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Gallery.Interfaces;

public interface IGalleryRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<GalleryAlbum?> GetAlbumByIdAsync(Guid albumId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<GalleryAlbum> Items, int TotalCount)> GetAlbumPageAsync(Guid schoolId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GalleryMedia>> GetMediaAsync(Guid albumId, CancellationToken cancellationToken);
    Task AddAlbumAsync(GalleryAlbum album, CancellationToken cancellationToken);
    Task AddMediaAsync(GalleryMedia media, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
