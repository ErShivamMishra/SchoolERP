using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Notices.Interfaces;

public interface INoticeRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<NoticeBoardItem?> GetByIdAsync(Guid noticeId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<NoticeBoardItem> Items, int TotalCount)> GetPageAsync(Guid schoolId, NoticeType? noticeType, NoticeAudienceType? audienceType, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(NoticeBoardItem notice, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
