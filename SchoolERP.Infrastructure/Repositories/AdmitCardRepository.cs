using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.AdmitCards.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AdmitCardRepository(SchoolErpDbContext dbContext) : IAdmitCardRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<AdmitCardTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken)
        => dbContext.AdmitCardTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);

    public async Task<int> GetNextTemplateVersionAsync(Guid schoolId, string templateName, CancellationToken cancellationToken)
        => (await dbContext.AdmitCardTemplates
            .Where(x => x.SchoolId == schoolId && x.TemplateName == templateName)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0) + 1;

    public Task AddTemplateAsync(AdmitCardTemplate template, CancellationToken cancellationToken)
        => dbContext.AdmitCardTemplates.AddAsync(template, cancellationToken).AsTask();

    public Task<Exam?> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken)
        => dbContext.Exams.FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

    public async Task<IReadOnlyCollection<Student>> GetStudentsAsync(Guid schoolId, IReadOnlyCollection<Guid> studentIds, Guid classId, Guid? sectionId, CancellationToken cancellationToken)
    {
        var query = dbContext.Students.Where(x => x.SchoolId == schoolId && studentIds.Contains(x.Id) && x.ClassId == classId && x.IsActive);
        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<GeneratedAdmitCard?> GetGeneratedCardAsync(Guid schoolId, Guid examId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.GeneratedAdmitCards.FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.ExamId == examId && x.StudentId == studentId, cancellationToken);

    public Task AddGeneratedCardAsync(GeneratedAdmitCard card, CancellationToken cancellationToken)
        => dbContext.GeneratedAdmitCards.AddAsync(card, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
