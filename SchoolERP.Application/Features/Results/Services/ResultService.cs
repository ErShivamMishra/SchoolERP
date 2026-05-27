using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Results.Interfaces;
using SchoolERP.Application.Features.Results.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Results.Services;

public sealed class ResultService(
    IResultRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateExamRequestDto> createValidator,
    IValidator<RecordExamResultsRequestDto> recordValidator,
    IValidator<GetExamListRequestDto> listValidator) : IResultService
{
    public async Task<ExamDto> CreateExamAsync(CreateExamRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureDependenciesAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, request.Subjects.Select(x => x.SubjectId).ToArray(), cancellationToken);

        var exam = new Exam
        {
            SchoolId = schoolId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            AcademicSessionId = request.AcademicSessionId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedBy = currentUserContext.UserId?.ToString(),
            Subjects = request.Subjects.Select(x => new ExamSubject
            {
                SubjectId = x.SubjectId,
                MaxMarks = x.MaxMarks,
                PassingMarks = x.PassingMarks,
                ExamDate = x.ExamDate,
                CreatedBy = currentUserContext.UserId?.ToString()
            }).ToList()
        };

        await repository.AddExamAsync(exam, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetExamByIdAsync(exam.Id, cancellationToken) ?? exam;
        await auditService.WriteAsync(ModuleCodes.ResultManagement, "ExamCreated", nameof(Exam), exam.Id.ToString(), "Success", "Exam created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapExam(saved);
    }

    public async Task<ExamDto> PublishExamAsync(Guid examId, PublishExamRequestDto request, CancellationToken cancellationToken)
    {
        var exam = await GetManagedExamAsync(examId, cancellationToken);
        exam.IsPublished = request.IsPublished;
        exam.ModifiedAtUtc = DateTime.UtcNow;
        exam.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.ResultManagement, "ExamPublishUpdated", nameof(Exam), exam.Id.ToString(), "Success", request.IsPublished ? "Exam published." : "Exam unpublished.", exam.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapExam(exam);
    }

    public async Task<IReadOnlyCollection<ExamResultDto>> RecordResultsAsync(Guid examId, RecordExamResultsRequestDto request, CancellationToken cancellationToken)
    {
        await recordValidator.ValidateAndThrowAsync(request, cancellationToken);
        var exam = await GetManagedExamAsync(examId, cancellationToken);
        var studentIds = request.Results.Select(x => x.StudentId).Distinct().ToArray();
        var students = await repository.GetStudentsByIdsAsync(exam.SchoolId, exam.ClassId, exam.SectionId, studentIds, cancellationToken);
        if (students.Count != studentIds.Length)
        {
            throw new BadRequestException("One or more students are invalid for the exam scope.", "invalid_exam_students");
        }

        foreach (var entry in request.Results)
        {
            var examSubject = exam.Subjects.FirstOrDefault(x => x.Id == entry.ExamSubjectId)
                ?? throw new NotFoundException("Exam subject not found.", "exam_subject_not_found");

            if (entry.ObtainedMarks > examSubject.MaxMarks)
            {
                throw new BadRequestException("Obtained marks cannot exceed max marks.", "invalid_obtained_marks");
            }

            var existing = await repository.GetExamResultAsync(entry.ExamSubjectId, entry.StudentId, cancellationToken);
            var result = existing ?? new ExamResult
            {
                SchoolId = exam.SchoolId,
                ExamId = exam.Id,
                ExamSubjectId = entry.ExamSubjectId,
                StudentId = entry.StudentId,
                CreatedBy = currentUserContext.UserId?.ToString()
            };

            result.ObtainedMarks = entry.ObtainedMarks;
            result.Grade = entry.Grade?.Trim();
            result.Remarks = entry.Remarks?.Trim();
            result.IsPublished = entry.IsPublished;
            result.ModifiedAtUtc = DateTime.UtcNow;
            result.ModifiedBy = currentUserContext.UserId?.ToString();
            await repository.AddOrUpdateExamResultAsync(result, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        var savedResults = await repository.GetExamResultsAsync(exam.Id, cancellationToken);
        await auditService.WriteAsync(ModuleCodes.ResultManagement, "ExamResultsRecorded", nameof(ExamResult), null, "Success", $"Recorded {request.Results.Count} exam results.", exam.SchoolId, currentUserContext.UserId, cancellationToken);
        return savedResults.Select(MapResult).ToArray();
    }

    public async Task<PagedResult<ExamDto>> GetExamsAsync(GetExamListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetExamPageAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, request.IsPublished, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<ExamDto>
        {
            Items = items.Select(MapExam).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ExamDto> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken)
        => MapExam(await GetManagedExamAsync(examId, cancellationToken));

    public async Task<StudentExamReportDto> GetStudentReportAsync(Guid examId, Guid studentId, CancellationToken cancellationToken)
    {
        var exam = await GetManagedExamAsync(examId, cancellationToken);
        var results = (await repository.GetExamResultsAsync(examId, cancellationToken)).Where(x => x.StudentId == studentId).ToArray();
        if (results.Length == 0)
        {
            throw new NotFoundException("Student exam report not found.", "student_exam_report_not_found");
        }

        var firstStudent = results[0].Student!;
        var totalObtained = results.Sum(x => x.ObtainedMarks);
        var totalMax = results.Sum(x => x.ExamSubject?.MaxMarks ?? 0);
        return new StudentExamReportDto
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            StudentId = studentId,
            StudentName = $"{firstStudent.FirstName} {firstStudent.LastName}".Trim(),
            RollNumber = firstStudent.RollNumber,
            TotalObtainedMarks = totalObtained,
            TotalMaxMarks = totalMax,
            Percentage = totalMax == 0 ? 0 : decimal.Round((totalObtained / totalMax) * 100m, 2),
            SubjectResults = results.Select(MapResult).ToArray()
        };
    }

    public async Task<IReadOnlyCollection<ExamResultDto>> GetClassResultsAsync(Guid examId, Guid? schoolId, CancellationToken cancellationToken)
    {
        var exam = await GetManagedExamWithScopeAsync(examId, schoolId, cancellationToken);
        return (await repository.GetExamResultsAsync(exam.Id, cancellationToken)).Select(MapResult).ToArray();
    }

    public async Task<ResultAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var analytics = await repository.GetAnalyticsAsync(resolvedSchoolId, cancellationToken);
        return new ResultAnalyticsDto
        {
            TotalExams = analytics.TotalExams,
            PublishedExams = analytics.PublishedExams,
            TotalResults = analytics.TotalResults,
            AveragePercentage = analytics.AveragePercentage,
            PassRate = analytics.TotalResults == 0 ? 0 : decimal.Round((analytics.PassedResults / (decimal)analytics.TotalResults) * 100m, 2)
        };
    }

    private async Task<Exam> GetManagedExamAsync(Guid examId, CancellationToken cancellationToken)
    {
        var exam = await repository.GetExamByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.", "exam_not_found");
        EnsureSchoolAccess(exam.SchoolId);
        return exam;
    }

    private async Task<Exam> GetManagedExamWithScopeAsync(Guid examId, Guid? schoolId, CancellationToken cancellationToken)
    {
        var exam = await repository.GetExamByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.", "exam_not_found");
        var expectedSchoolId = ResolveSchoolIdForRead(schoolId);
        if (exam.SchoolId != expectedSchoolId)
        {
            throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return exam;
    }

    private async Task EnsureDependenciesAsync(Guid schoolId, Guid classId, Guid? sectionId, Guid academicSessionId, IReadOnlyCollection<Guid> subjectIds, CancellationToken cancellationToken)
    {
        var classEntity = await repository.GetClassByIdAsync(classId, cancellationToken) ?? throw new NotFoundException("Class not found.", "class_not_found");
        var session = await repository.GetAcademicSessionByIdAsync(academicSessionId, cancellationToken) ?? throw new NotFoundException("Academic session not found.", "academic_session_not_found");
        if (classEntity.TenantId != schoolId || session.TenantId != schoolId)
        {
            throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        if (sectionId.HasValue)
        {
            var section = await repository.GetSectionByIdAsync(sectionId.Value, cancellationToken) ?? throw new NotFoundException("Section not found.", "section_not_found");
            if (section.TenantId != schoolId || section.ClassId != classId)
            {
                throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
            }
        }

        var subjects = await repository.GetSubjectsByIdsAsync(schoolId, subjectIds, cancellationToken);
        if (subjects.Count != subjectIds.Count)
        {
            throw new BadRequestException("One or more subjects are invalid for the school.", "invalid_exam_subjects");
        }
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
            throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Result access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static ExamDto MapExam(Exam exam) => new()
    {
        Id = exam.Id,
        SchoolId = exam.SchoolId,
        ClassId = exam.ClassId,
        ClassName = exam.Class?.Name ?? string.Empty,
        SectionId = exam.SectionId,
        SectionName = exam.Section?.Name,
        AcademicSessionId = exam.AcademicSessionId,
        AcademicSessionName = exam.AcademicSession?.Name ?? string.Empty,
        Title = exam.Title,
        Description = exam.Description,
        StartDate = exam.StartDate,
        EndDate = exam.EndDate,
        IsPublished = exam.IsPublished,
        Subjects = exam.Subjects.Select(x => new ExamSubjectDto
        {
            Id = x.Id,
            SubjectId = x.SubjectId,
            SubjectName = x.Subject?.Name ?? string.Empty,
            MaxMarks = x.MaxMarks,
            PassingMarks = x.PassingMarks,
            ExamDate = x.ExamDate
        }).ToArray()
    };

    private static ExamResultDto MapResult(ExamResult result) => new()
    {
        Id = result.Id,
        ExamId = result.ExamId,
        ExamSubjectId = result.ExamSubjectId,
        SubjectName = result.ExamSubject?.Subject?.Name ?? string.Empty,
        StudentId = result.StudentId,
        StudentName = result.Student is null ? string.Empty : $"{result.Student.FirstName} {result.Student.LastName}".Trim(),
        RollNumber = result.Student?.RollNumber ?? string.Empty,
        ObtainedMarks = result.ObtainedMarks,
        MaxMarks = result.ExamSubject?.MaxMarks ?? 0,
        PassingMarks = result.ExamSubject?.PassingMarks ?? 0,
        Grade = result.Grade,
        Remarks = result.Remarks,
        IsPublished = result.IsPublished
    };
}
