using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Communications.Models;

namespace SchoolERP.Application.Features.Communications.Interfaces;

public interface ICommunicationService
{
    Task<ConversationDto> CreateConversationAsync(CreateConversationRequestDto request, CancellationToken cancellationToken);
    Task<ConversationMessageDto> SendMessageAsync(Guid conversationId, SendMessageRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<ConversationDto>> GetConversationsAsync(GetConversationListRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ConversationMessageDto>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken);
}
