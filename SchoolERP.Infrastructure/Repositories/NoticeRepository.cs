using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Notices.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class NoticeRepository(SchoolErpDbContext dbContext) : INoticeRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<NoticeBoardItem?> GetByIdAsync(Guid noticeId, CancellationToken cancellationToken)
        => dbContext.NoticeBoardItems.Include(x => x.Class).Include(x => x.Section).FirstOrDefaultAsync(x => x.Id == noticeId, cancellationToken);

    public async Task<(IReadOnlyCollection<NoticeBoardItem> Items, int TotalCount)> GetPageAsync(Guid schoolId, NoticeType? noticeType, NoticeAudienceType? audienceType, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.NoticeBoardItems.Include(x => x.Class).Include(x => x.Section).Where(x => x.SchoolId == schoolId);
        if (noticeType.HasValue) query = query.Where(x => x.NoticeType == noticeType.Value);
        if (audienceType.HasValue) query = query.Where(x => x.AudienceType == audienceType.Value);
        if (isPublished.HasValue) query = query.Where(x => x.IsPublished == isPublished.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Content.Contains(term));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task AddAsync(NoticeBoardItem notice, CancellationToken cancellationToken)
        => dbContext.NoticeBoardItems.AddAsync(notice, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
