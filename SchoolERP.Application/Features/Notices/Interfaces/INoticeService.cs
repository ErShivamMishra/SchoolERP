using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Notices.Models;

namespace SchoolERP.Application.Features.Notices.Interfaces;

public interface INoticeService
{
    Task<NoticeDto> CreateAsync(CreateNoticeRequestDto request, CancellationToken cancellationToken);
    Task<NoticeDto> PublishAsync(Guid noticeId, PublishNoticeRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<NoticeDto>> GetAllAsync(GetNoticeListRequestDto request, CancellationToken cancellationToken);
    Task<NoticeDto> GetByIdAsync(Guid noticeId, CancellationToken cancellationToken);
}
