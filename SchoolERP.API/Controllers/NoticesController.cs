using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Requests;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Notices.Interfaces;
using SchoolERP.Application.Features.Notices.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Enums;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/notices")]
public sealed class NoticesController(INoticeService noticeService) : ControllerBase
{
    [HttpPost]
    [ModuleAccess(ModuleCodes.NoticeBoardManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromForm] CreateNoticeFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await noticeService.CreateAsync(new CreateNoticeRequestDto
        {
            SchoolId = request.SchoolId,
            Title = request.Title,
            Content = request.Content,
            NoticeType = (NoticeType)request.NoticeType,
            AudienceType = (NoticeAudienceType)request.AudienceType,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            ExpiryDate = request.ExpiryDate,
            Attachment = await ToPayloadAsync(request.Attachment, cancellationToken)
        }, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Notice created successfully."));
    }

    [HttpPatch("{noticeId:guid}/publish")]
    [ModuleAccess(ModuleCodes.NoticeBoardManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(Guid noticeId, [FromBody] PublishNoticeRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await noticeService.PublishAsync(noticeId, request, cancellationToken), "Notice publication status updated successfully."));

    [HttpGet]
    [ModuleAccess(ModuleCodes.NoticeBoardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NoticeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetNoticeListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await noticeService.GetAllAsync(request, cancellationToken), "Notices fetched successfully."));

    [HttpGet("{noticeId:guid}")]
    [ModuleAccess(ModuleCodes.NoticeBoardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid noticeId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await noticeService.GetByIdAsync(noticeId, cancellationToken), "Notice fetched successfully."));

    private static async Task<FileUploadPayload?> ToPayloadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return new FileUploadPayload
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Content = memory.ToArray()
        };
    }
}
