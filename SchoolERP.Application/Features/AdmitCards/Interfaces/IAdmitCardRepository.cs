using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.AdmitCards.Interfaces;

public interface IAdmitCardRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<AdmitCardTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken);
    Task<int> GetNextTemplateVersionAsync(Guid schoolId, string templateName, CancellationToken cancellationToken);
    Task AddTemplateAsync(AdmitCardTemplate template, CancellationToken cancellationToken);
    Task<Exam?> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> GetStudentsAsync(Guid schoolId, IReadOnlyCollection<Guid> studentIds, Guid classId, Guid? sectionId, CancellationToken cancellationToken);
    Task<GeneratedAdmitCard?> GetGeneratedCardAsync(Guid schoolId, Guid examId, Guid studentId, CancellationToken cancellationToken);
    Task AddGeneratedCardAsync(GeneratedAdmitCard card, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
