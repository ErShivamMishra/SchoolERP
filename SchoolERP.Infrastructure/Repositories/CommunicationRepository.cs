using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Communications.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class CommunicationRepository(SchoolErpDbContext dbContext) : ICommunicationRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

    public Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken)
        => dbContext.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId, cancellationToken);

    public Task<ParentTeacherConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken)
        => dbContext.ParentTeacherConversations
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

    public Task<ParentTeacherConversation?> GetConversationAsync(Guid schoolId, Guid studentId, Guid teacherId, string subject, CancellationToken cancellationToken)
        => dbContext.ParentTeacherConversations.FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.StudentId == studentId && x.TeacherId == teacherId && x.Subject == subject, cancellationToken);

    public async Task<(IReadOnlyCollection<ParentTeacherConversation> Items, int TotalCount)> GetConversationPageAsync(Guid schoolId, Guid? teacherId, Guid? studentId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.ParentTeacherConversations.Include(x => x.Student).Include(x => x.Teacher).Where(x => x.SchoolId == schoolId);
        if (teacherId.HasValue) query = query.Where(x => x.TeacherId == teacherId.Value);
        if (studentId.HasValue) query = query.Where(x => x.StudentId == studentId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Subject.Contains(term) || x.Student!.FirstName.Contains(term) || x.Teacher!.FirstName.Contains(term));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.LastMessageAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<ParentTeacherMessage>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken)
        => await dbContext.ParentTeacherMessages.Where(x => x.ConversationId == conversationId).OrderBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task AddConversationAsync(ParentTeacherConversation conversation, CancellationToken cancellationToken)
        => dbContext.ParentTeacherConversations.AddAsync(conversation, cancellationToken).AsTask();

    public Task AddMessageAsync(ParentTeacherMessage message, CancellationToken cancellationToken)
        => dbContext.ParentTeacherMessages.AddAsync(message, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
