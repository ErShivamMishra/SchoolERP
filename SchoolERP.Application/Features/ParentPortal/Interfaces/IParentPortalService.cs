using SchoolERP.Application.Features.ParentPortal.Models;

namespace SchoolERP.Application.Features.ParentPortal.Interfaces;

public interface IParentPortalService
{
    Task<CreateParentResultDto> CreateParentAsync(CreateParentRequestDto request, CancellationToken cancellationToken);
    Task<ParentLinkedStudentDto> LinkStudentAsync(Guid parentId, LinkParentStudentRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentLinkedStudentDto>> GetMyStudentsAsync(CancellationToken cancellationToken);
    Task<ParentAttendanceSummaryDto> GetMyStudentAttendanceAsync(Guid studentId, CancellationToken cancellationToken);
    Task<ParentFeeStatusDto> GetMyStudentFeeStatusAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentResultSummaryDto>> GetMyStudentResultsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentHomeworkDto>> GetMyStudentHomeworkAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentNoticeDto>> GetMyNoticesAsync(CancellationToken cancellationToken);
}
