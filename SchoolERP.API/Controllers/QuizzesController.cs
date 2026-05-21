using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Quizzes.Interfaces;
using SchoolERP.Application.Features.Quizzes.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/quizzes")]
public sealed class QuizzesController(IQuizService quizService) : ControllerBase
{
    [HttpPost]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<QuizDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateQuizRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.CreateAsync(request, cancellationToken), "Quiz created successfully."));

    [HttpPatch("{quizId:guid}/publish")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<QuizDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(Guid quizId, [FromBody] PublishQuizRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.PublishAsync(quizId, request, cancellationToken), "Quiz status updated successfully."));

    [HttpPost("{quizId:guid}/students/{studentId:guid}/submit")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<QuizResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(Guid quizId, Guid studentId, [FromBody] SubmitQuizRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.SubmitAsync(quizId, studentId, request, cancellationToken), "Quiz submitted successfully."));

    [HttpPatch("{quizId:guid}/students/{studentId:guid}/evaluate")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<QuizResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Evaluate(Guid quizId, Guid studentId, [FromBody] ManualQuizEvaluationRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.ManualEvaluateAsync(quizId, studentId, request, cancellationToken), "Quiz evaluated successfully."));

    [HttpGet]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<QuizDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QuizListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.GetAllAsync(request, cancellationToken), "Quizzes fetched successfully."));

    [HttpGet("{quizId:guid}")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<QuizDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid quizId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.GetByIdAsync(quizId, cancellationToken), "Quiz fetched successfully."));

    [HttpGet("{quizId:guid}/leaderboard")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<QuizLeaderboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Leaderboard(Guid quizId, [FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.GetLeaderboardAsync(quizId, schoolId, cancellationToken), "Quiz leaderboard fetched successfully."));

    [HttpGet("analytics")]
    [ModuleAccess(ModuleCodes.QuizManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<QuizAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analytics([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await quizService.GetAnalyticsAsync(schoolId, cancellationToken), "Quiz analytics fetched successfully."));
}
