using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Quizzes.Models;

namespace SchoolERP.Application.Features.Quizzes.Interfaces;

public interface IQuizService
{
    Task<QuizDto> CreateAsync(CreateQuizRequestDto request, CancellationToken cancellationToken);
    Task<QuizDto> PublishAsync(Guid quizId, PublishQuizRequestDto request, CancellationToken cancellationToken);
    Task<QuizResultDto> SubmitAsync(Guid quizId, Guid studentId, SubmitQuizRequestDto request, CancellationToken cancellationToken);
    Task<QuizResultDto> ManualEvaluateAsync(Guid quizId, Guid studentId, ManualQuizEvaluationRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<QuizDto>> GetAllAsync(QuizListRequestDto request, CancellationToken cancellationToken);
    Task<QuizDto> GetByIdAsync(Guid quizId, CancellationToken cancellationToken);
    Task<QuizLeaderboardDto> GetLeaderboardAsync(Guid quizId, Guid? schoolId, CancellationToken cancellationToken);
    Task<QuizAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken);
}
