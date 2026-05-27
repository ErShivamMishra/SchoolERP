using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Communications.Interfaces;

public interface ICommunicationRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<ParentTeacherConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken);
    Task<ParentTeacherConversation?> GetConversationAsync(Guid schoolId, Guid studentId, Guid teacherId, string subject, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<ParentTeacherConversation> Items, int TotalCount)> GetConversationPageAsync(Guid schoolId, Guid? teacherId, Guid? studentId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentTeacherMessage>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken);
    Task AddConversationAsync(ParentTeacherConversation conversation, CancellationToken cancellationToken);
    Task AddMessageAsync(ParentTeacherMessage message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
