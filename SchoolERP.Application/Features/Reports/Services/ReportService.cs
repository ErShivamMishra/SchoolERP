using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Models;
using SchoolERP.Application.Features.Reports.Interfaces;
using SchoolERP.Application.Features.Reports.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.Application.Features.Reports.Services;

public sealed class ReportService(
    IReportRepository repository,
    ICurrentUserContext currentUserContext,
    IValidator<StudentExportRequestDto> studentValidator,
    IValidator<AttendanceExportRequestDto> attendanceValidator,
    IValidator<QuizResultExportRequestDto> quizValidator) : IReportService
{
    public async Task<PagedResult<StudentExportRowDto>> ExportStudentsAsync(StudentExportRequestDto request, CancellationToken cancellationToken)
    {
        await studentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetStudentsPageAsync(schoolId, request.ClassId, request.SectionId, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<StudentExportRowDto>
        {
            Items = items.Select(x => new StudentExportRowDto
            {
                RollNumber = x.RollNumber,
                StudentName = $"{x.FirstName} {x.LastName}".Trim(),
                ClassName = x.Class?.Name ?? string.Empty,
                SectionName = x.Section?.Name ?? string.Empty,
                MobileNumber = x.MobileNumber,
                Email = x.Email,
                IsActive = x.IsActive
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<AttendanceExportRowDto>> ExportAttendanceAsync(AttendanceExportRequestDto request, CancellationToken cancellationToken)
    {
        await attendanceValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetAttendancePageAsync(schoolId, request.FromDate, request.ToDate, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<AttendanceExportRowDto>
        {
            Items = items.Select(x => new AttendanceExportRowDto
            {
                AttendanceDate = x.AttendanceDate,
                StudentName = $"{x.Student?.FirstName} {x.Student?.LastName}".Trim(),
                RollNumber = x.Student?.RollNumber ?? string.Empty,
                ClassName = x.Class?.Name ?? string.Empty,
                SectionName = x.Section?.Name ?? string.Empty,
                Status = x.Status.ToString(),
                Remarks = x.Remarks
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<FeeExportRowDto>> ExportFeesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
    {
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetInvoicePageAsync(schoolId, request.StudentId, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<FeeExportRowDto>
        {
            Items = items.Select(x => new FeeExportRowDto
            {
                InvoiceNumber = x.InvoiceNumber,
                StudentName = $"{x.Student?.FirstName} {x.Student?.LastName}".Trim(),
                RollNumber = x.Student?.RollNumber ?? string.Empty,
                InvoiceDate = x.InvoiceDate,
                DueDate = x.DueDate,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.PaidAmount,
                PendingAmount = x.PendingAmount,
                Status = x.Status.ToString()
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<QuizResultExportRowDto>> ExportQuizResultsAsync(QuizResultExportRequestDto request, CancellationToken cancellationToken)
    {
        await quizValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetQuizResultPageAsync(schoolId, request.QuizId, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<QuizResultExportRowDto>
        {
            Items = items.Select(x => new QuizResultExportRowDto
            {
                StudentName = $"{x.Student?.FirstName} {x.Student?.LastName}".Trim(),
                RollNumber = x.Student?.RollNumber ?? string.Empty,
                ObtainedMarks = x.ObtainedMarks,
                TotalMarks = x.TotalMarks,
                Percentage = x.Percentage,
                IsPassed = x.IsPassed,
                Rank = x.Rank
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
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
            throw new ForbiddenException("Report access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }
}
