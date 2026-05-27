using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Results.Models;

namespace SchoolERP.Application.Features.Results.Interfaces;

public interface IResultService
{
    Task<ExamDto> CreateExamAsync(CreateExamRequestDto request, CancellationToken cancellationToken);
    Task<ExamDto> PublishExamAsync(Guid examId, PublishExamRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExamResultDto>> RecordResultsAsync(Guid examId, RecordExamResultsRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<ExamDto>> GetExamsAsync(GetExamListRequestDto request, CancellationToken cancellationToken);
    Task<ExamDto> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken);
    Task<StudentExamReportDto> GetStudentReportAsync(Guid examId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExamResultDto>> GetClassResultsAsync(Guid examId, Guid? schoolId, CancellationToken cancellationToken);
    Task<ResultAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken);
}
