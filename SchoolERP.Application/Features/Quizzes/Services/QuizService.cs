using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Quizzes.Interfaces;
using SchoolERP.Application.Features.Quizzes.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using System.Text.Json;

namespace SchoolERP.Application.Features.Quizzes.Services;

public sealed class QuizService(
    IQuizRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateQuizRequestDto> createValidator,
    IValidator<PublishQuizRequestDto> publishValidator,
    IValidator<SubmitQuizRequestDto> submitValidator,
    IValidator<ManualQuizEvaluationRequestDto> manualValidator,
    IValidator<QuizListRequestDto> listValidator) : IQuizService
{
    public async Task<QuizDto> CreateAsync(CreateQuizRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var subject = await repository.GetSubjectByIdAsync(request.SubjectId, cancellationToken) ?? throw new NotFoundException("Subject not found.", "subject_not_found");
        var classEntity = await repository.GetClassByIdAsync(request.ClassId, cancellationToken) ?? throw new NotFoundException("Class not found.", "class_not_found");
        if (subject.SchoolId != schoolId || classEntity.TenantId != schoolId)
        {
            throw new ForbiddenException("Quiz access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        if (request.SectionId.HasValue)
        {
            var section = await repository.GetSectionByIdAsync(request.SectionId.Value, cancellationToken) ?? throw new NotFoundException("Section not found.", "section_not_found");
            if (section.TenantId != schoolId)
            {
                throw new ForbiddenException("Quiz access is limited to the current school.", "cross_tenant_access_forbidden");
            }
        }

        var quiz = new Quiz
        {
            SchoolId = schoolId,
            Title = request.Title.Trim(),
            SubjectId = request.SubjectId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            TotalMarks = request.TotalMarks,
            PassingMarks = request.PassingMarks,
            DurationMinutes = request.DurationMinutes,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RandomizeQuestions = request.RandomizeQuestions,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        quiz.Questions = request.Questions.Select(question => new QuizQuestion
        {
            QuizId = quiz.Id,
            QuestionText = question.QuestionText.Trim(),
            QuestionType = question.QuestionType,
            Marks = question.Marks,
            DisplayOrder = question.DisplayOrder,
            CreatedBy = currentUserContext.UserId?.ToString(),
            Options = question.Options.Select((option, index) => new QuizOption
            {
                QuestionId = Guid.Empty,
                OptionText = option.OptionText.Trim(),
                IsCorrect = option.IsCorrect,
                DisplayOrder = index + 1,
                CreatedBy = currentUserContext.UserId?.ToString()
            }).ToList()
        }).ToList();

        await repository.AddQuizAsync(quiz, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        quiz.Subject = subject;
        await auditService.WriteAsync(ModuleCodes.QuizManagement, "QuizCreated", nameof(Quiz), quiz.Id.ToString(), "Success", "Quiz created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapQuiz(quiz);
    }

    public async Task<QuizDto> PublishAsync(Guid quizId, PublishQuizRequestDto request, CancellationToken cancellationToken)
    {
        await publishValidator.ValidateAndThrowAsync(request, cancellationToken);
        var quiz = await repository.GetQuizByIdAsync(quizId, cancellationToken) ?? throw new NotFoundException("Quiz not found.", "quiz_not_found");
        EnsureSchoolAccess(quiz.SchoolId);
        quiz.IsPublished = request.IsPublished;
        quiz.ModifiedAtUtc = DateTime.UtcNow;
        quiz.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.QuizManagement, "QuizPublishUpdated", nameof(Quiz), quiz.Id.ToString(), "Success", request.IsPublished ? "Quiz published." : "Quiz unpublished.", quiz.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapQuiz(quiz);
    }

    public async Task<QuizResultDto> SubmitAsync(Guid quizId, Guid studentId, SubmitQuizRequestDto request, CancellationToken cancellationToken)
    {
        await submitValidator.ValidateAndThrowAsync(request, cancellationToken);
        var quiz = await repository.GetQuizByIdAsync(quizId, cancellationToken) ?? throw new NotFoundException("Quiz not found.", "quiz_not_found");
        EnsureSchoolAccess(quiz.SchoolId);
        if (!quiz.IsPublished)
        {
            throw new BadRequestException("Quiz is not published yet.", "quiz_not_published");
        }

        var now = DateTime.UtcNow;
        if (now < quiz.StartDate || now > quiz.EndDate)
        {
            throw new BadRequestException("Quiz submissions are allowed only during the scheduled window.", "quiz_submission_window_closed");
        }

        if (await repository.GetSubmissionAsync(quizId, studentId, cancellationToken) is not null)
        {
            throw new ConflictException("Only one submission is allowed per student.", "quiz_already_submitted");
        }

        var student = await repository.GetStudentByIdAsync(studentId, cancellationToken) ?? throw new NotFoundException("Student not found.", "student_not_found");
        EnsureSchoolAccess(student.SchoolId);

        decimal autoScore = 0;
        foreach (var answer in request.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(x => x.Id == answer.QuestionId);
            if (question is null)
            {
                continue;
            }

            if (question.QuestionType == QuizQuestionType.MultipleChoice && answer.SelectedOptionId.HasValue)
            {
                var selectedOption = question.Options.FirstOrDefault(x => x.Id == answer.SelectedOptionId.Value);
                if (selectedOption?.IsCorrect == true)
                {
                    autoScore += question.Marks;
                }
            }
        }

        var submission = new QuizSubmission
        {
            SchoolId = quiz.SchoolId,
            QuizId = quizId,
            StudentId = studentId,
            StartedAt = request.StartedAt,
            SubmittedAt = now,
            Status = QuizSubmissionStatus.Evaluated,
            AnswersJson = JsonSerializer.Serialize(request.Answers),
            AutoEvaluatedMarks = autoScore,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddSubmissionAsync(submission, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var result = new QuizResult
        {
            SchoolId = quiz.SchoolId,
            QuizId = quiz.Id,
            StudentId = studentId,
            QuizSubmissionId = submission.Id,
            ObtainedMarks = autoScore,
            TotalMarks = quiz.TotalMarks,
            Percentage = quiz.TotalMarks == 0 ? 0 : decimal.Round((autoScore / quiz.TotalMarks) * 100m, 2),
            IsPassed = autoScore >= quiz.PassingMarks,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddOrUpdateResultAsync(result, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var leaderboard = await repository.GetLeaderboardAsync(quiz.SchoolId, quizId, cancellationToken);
        ApplyRanks(leaderboard);
        await repository.SaveChangesAsync(cancellationToken);
        result.Student = student;
        await auditService.WriteAsync(ModuleCodes.QuizManagement, "QuizSubmitted", nameof(QuizSubmission), submission.Id.ToString(), "Success", "Quiz submitted.", quiz.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapResult(result);
    }

    public async Task<QuizResultDto> ManualEvaluateAsync(Guid quizId, Guid studentId, ManualQuizEvaluationRequestDto request, CancellationToken cancellationToken)
    {
        await manualValidator.ValidateAndThrowAsync(request, cancellationToken);
        var quiz = await repository.GetQuizByIdAsync(quizId, cancellationToken) ?? throw new NotFoundException("Quiz not found.", "quiz_not_found");
        EnsureSchoolAccess(quiz.SchoolId);
        var result = await repository.GetResultAsync(quizId, studentId, cancellationToken) ?? throw new NotFoundException("Quiz result not found.", "quiz_result_not_found");
        var updatedMarks = Math.Max(0, result.QuizSubmission?.AutoEvaluatedMarks ?? result.ObtainedMarks);
        updatedMarks += request.ManualAdjustmentMarks;
        result.ObtainedMarks = updatedMarks;
        result.TotalMarks = quiz.TotalMarks;
        result.Percentage = quiz.TotalMarks == 0 ? 0 : decimal.Round((updatedMarks / quiz.TotalMarks) * 100m, 2);
        result.IsPassed = updatedMarks >= quiz.PassingMarks;
        result.EvaluatorRemarks = request.EvaluatorRemarks?.Trim();
        result.ModifiedAtUtc = DateTime.UtcNow;
        result.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        var leaderboard = await repository.GetLeaderboardAsync(quiz.SchoolId, quizId, cancellationToken);
        ApplyRanks(leaderboard);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.QuizManagement, "QuizManuallyEvaluated", nameof(QuizResult), result.Id.ToString(), "Success", "Quiz result manually adjusted.", quiz.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapResult(result);
    }

    public async Task<PagedResult<QuizDto>> GetAllAsync(QuizListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetQuizPageAsync(schoolId, request.ClassId, request.SectionId, request.SubjectId, request.IsPublished, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<QuizDto>
        {
            Items = items.Select(MapQuiz).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<QuizDto> GetByIdAsync(Guid quizId, CancellationToken cancellationToken)
    {
        var quiz = await repository.GetQuizByIdAsync(quizId, cancellationToken) ?? throw new NotFoundException("Quiz not found.", "quiz_not_found");
        EnsureSchoolAccess(quiz.SchoolId);
        return MapQuiz(quiz);
    }

    public async Task<QuizLeaderboardDto> GetLeaderboardAsync(Guid quizId, Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var leaderboard = await repository.GetLeaderboardAsync(resolvedSchoolId, quizId, cancellationToken);
        ApplyRanks(leaderboard);
        await repository.SaveChangesAsync(cancellationToken);
        var quiz = await repository.GetQuizByIdAsync(quizId, cancellationToken) ?? throw new NotFoundException("Quiz not found.", "quiz_not_found");
        return new QuizLeaderboardDto
        {
            QuizId = quizId,
            QuizTitle = quiz.Title,
            Rankings = leaderboard.Select(MapResult).ToArray()
        };
    }

    public async Task<QuizAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var analytics = await repository.GetAnalyticsAsync(resolvedSchoolId, cancellationToken);
        return new QuizAnalyticsDto
        {
            TotalQuizzes = analytics.TotalQuizzes,
            PublishedQuizzes = analytics.PublishedQuizzes,
            TotalSubmissions = analytics.TotalSubmissions,
            AverageScore = analytics.AverageScore,
            ParticipationRate = analytics.ActiveStudents == 0 ? 0 : decimal.Round((analytics.TotalSubmissions / (decimal)analytics.ActiveStudents) * 100m, 2)
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
            throw new ForbiddenException("Quiz access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Quiz access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Quiz access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static void ApplyRanks(IReadOnlyCollection<QuizResult> results)
    {
        var rank = 1;
        foreach (var item in results.OrderByDescending(x => x.ObtainedMarks).ThenBy(x => x.CreatedAtUtc))
        {
            item.Rank = rank++;
        }
    }

    private static QuizDto MapQuiz(Quiz quiz) => new()
    {
        Id = quiz.Id,
        SchoolId = quiz.SchoolId,
        Title = quiz.Title,
        SubjectId = quiz.SubjectId,
        SubjectName = quiz.Subject?.Name ?? string.Empty,
        ClassId = quiz.ClassId,
        SectionId = quiz.SectionId,
        TotalMarks = quiz.TotalMarks,
        PassingMarks = quiz.PassingMarks,
        DurationMinutes = quiz.DurationMinutes,
        StartDate = quiz.StartDate,
        EndDate = quiz.EndDate,
        IsPublished = quiz.IsPublished,
        RandomizeQuestions = quiz.RandomizeQuestions,
        Questions = quiz.Questions.OrderBy(x => x.DisplayOrder).Select(question => new QuizQuestionDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType,
            Marks = question.Marks,
            DisplayOrder = question.DisplayOrder,
            Options = question.Options.OrderBy(x => x.DisplayOrder).Select(option => new QuizOptionDto
            {
                Id = option.Id,
                OptionText = option.OptionText
            }).ToArray()
        }).ToArray()
    };

    private static QuizResultDto MapResult(QuizResult result) => new()
    {
        QuizId = result.QuizId,
        StudentId = result.StudentId,
        StudentName = $"{result.Student?.FirstName} {result.Student?.LastName}".Trim(),
        RollNumber = result.Student?.RollNumber ?? string.Empty,
        ObtainedMarks = result.ObtainedMarks,
        TotalMarks = result.TotalMarks,
        Percentage = result.Percentage,
        IsPassed = result.IsPassed,
        Rank = result.Rank,
        EvaluatorRemarks = result.EvaluatorRemarks
    };
}
