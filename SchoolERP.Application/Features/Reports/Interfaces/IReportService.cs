using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Models;
using SchoolERP.Application.Features.Reports.Models;

namespace SchoolERP.Application.Features.Reports.Interfaces;

public interface IReportService
{
    Task<PagedResult<StudentExportRowDto>> ExportStudentsAsync(StudentExportRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<AttendanceExportRowDto>> ExportAttendanceAsync(AttendanceExportRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<FeeExportRowDto>> ExportFeesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<QuizResultExportRowDto>> ExportQuizResultsAsync(QuizResultExportRequestDto request, CancellationToken cancellationToken);
}
