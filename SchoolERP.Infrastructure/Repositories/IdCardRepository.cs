using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.IdCards.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class IdCardRepository(SchoolErpDbContext dbContext) : IIdCardRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<IdCardTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken)
        => dbContext.IdCardTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);

    public async Task<int> GetNextTemplateVersionAsync(Guid schoolId, string templateName, CancellationToken cancellationToken)
        => (await dbContext.IdCardTemplates
            .Where(x => x.SchoolId == schoolId && x.TemplateName == templateName)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0) + 1;

    public Task AddTemplateAsync(IdCardTemplate template, CancellationToken cancellationToken)
        => dbContext.IdCardTemplates.AddAsync(template, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Student>> GetStudentsAsync(Guid schoolId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken)
        => await dbContext.Students
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Where(x => x.SchoolId == schoolId && studentIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(Guid schoolId, IReadOnlyCollection<Guid> teacherIds, CancellationToken cancellationToken)
        => await dbContext.Teachers
            .Where(x => x.SchoolId == schoolId && teacherIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

    public Task<GeneratedIdCard?> GetGeneratedStudentCardAsync(Guid schoolId, Guid templateId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.GeneratedIdCards.FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.TemplateId == templateId && x.StudentId == studentId, cancellationToken);

    public Task<GeneratedIdCard?> GetGeneratedTeacherCardAsync(Guid schoolId, Guid templateId, Guid teacherId, CancellationToken cancellationToken)
        => dbContext.GeneratedIdCards.FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.TemplateId == templateId && x.TeacherId == teacherId, cancellationToken);

    public Task AddGeneratedCardAsync(GeneratedIdCard card, CancellationToken cancellationToken)
        => dbContext.GeneratedIdCards.AddAsync(card, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
