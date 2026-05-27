using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Requests;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Communications.Interfaces;
using SchoolERP.Application.Features.Communications.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Enums;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/communications")]
public sealed class CommunicationsController(ICommunicationService communicationService) : ControllerBase
{
    [HttpPost("conversations")]
    [ModuleAccess(ModuleCodes.CommunicationManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await communicationService.CreateConversationAsync(request, cancellationToken), "Conversation created successfully."));

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [ModuleAccess(ModuleCodes.CommunicationManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<ConversationMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromForm] SendConversationMessageFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await communicationService.SendMessageAsync(conversationId, new SendMessageRequestDto
        {
            MessageText = request.MessageText,
            SenderType = (MessageSenderType)request.SenderType,
            Attachment = await ToPayloadAsync(request.Attachment, cancellationToken)
        }, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Conversation message sent successfully."));
    }

    [HttpGet("conversations")]
    [ModuleAccess(ModuleCodes.CommunicationManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ConversationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations([FromQuery] GetConversationListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await communicationService.GetConversationsAsync(request, cancellationToken), "Conversations fetched successfully."));

    [HttpGet("conversations/{conversationId:guid}/messages")]
    [ModuleAccess(ModuleCodes.CommunicationManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ConversationMessageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(Guid conversationId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await communicationService.GetMessagesAsync(conversationId, cancellationToken), "Conversation history fetched successfully."));

    private static async Task<FileUploadPayload?> ToPayloadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return null;
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
