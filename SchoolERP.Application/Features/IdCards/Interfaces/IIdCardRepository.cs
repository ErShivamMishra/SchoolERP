using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.IdCards.Interfaces;

public interface IIdCardRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IdCardTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken);
    Task<int> GetNextTemplateVersionAsync(Guid schoolId, string templateName, CancellationToken cancellationToken);
    Task AddTemplateAsync(IdCardTemplate template, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> GetStudentsAsync(Guid schoolId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(Guid schoolId, IReadOnlyCollection<Guid> teacherIds, CancellationToken cancellationToken);
    Task<GeneratedIdCard?> GetGeneratedStudentCardAsync(Guid schoolId, Guid templateId, Guid studentId, CancellationToken cancellationToken);
    Task<GeneratedIdCard?> GetGeneratedTeacherCardAsync(Guid schoolId, Guid templateId, Guid teacherId, CancellationToken cancellationToken);
    Task AddGeneratedCardAsync(GeneratedIdCard card, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
