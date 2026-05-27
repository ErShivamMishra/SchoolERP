using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Gallery.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class GalleryRepository(SchoolErpDbContext dbContext) : IGalleryRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<GalleryAlbum?> GetAlbumByIdAsync(Guid albumId, CancellationToken cancellationToken)
        => dbContext.GalleryAlbums.Include(x => x.MediaItems).FirstOrDefaultAsync(x => x.Id == albumId, cancellationToken);

    public async Task<(IReadOnlyCollection<GalleryAlbum> Items, int TotalCount)> GetAlbumPageAsync(Guid schoolId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.GalleryAlbums.Include(x => x.MediaItems).Where(x => x.SchoolId == schoolId);
        if (isPublished.HasValue) query = query.Where(x => x.IsPublished == isPublished.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<GalleryMedia>> GetMediaAsync(Guid albumId, CancellationToken cancellationToken)
        => await dbContext.GalleryMedia.Where(x => x.AlbumId == albumId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task AddAlbumAsync(GalleryAlbum album, CancellationToken cancellationToken)
        => dbContext.GalleryAlbums.AddAsync(album, cancellationToken).AsTask();

    public Task AddMediaAsync(GalleryMedia media, CancellationToken cancellationToken)
        => dbContext.GalleryMedia.AddAsync(media, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
