using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Study.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudyRepository(SchoolErpDbContext dbContext) : IStudyRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<bool> SubjectCodeExistsAsync(Guid schoolId, string code, Guid? excludeSubjectId, CancellationToken cancellationToken)
        => dbContext.Subjects.AnyAsync(x => x.SchoolId == schoolId && x.Code == code && (!excludeSubjectId.HasValue || x.Id != excludeSubjectId.Value), cancellationToken);

    public Task AddSubjectAsync(Subject subject, CancellationToken cancellationToken)
        => dbContext.Subjects.AddAsync(subject, cancellationToken).AsTask();

    public Task<Subject?> GetSubjectByIdAsync(Guid subjectId, CancellationToken cancellationToken)
        => dbContext.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId, cancellationToken);

    public async Task<IReadOnlyCollection<Subject>> GetSubjectsAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.Subjects.Where(x => x.SchoolId == schoolId).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken)
        => dbContext.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId, cancellationToken);

    public Task<Syllabus?> GetSyllabusAsync(Guid schoolId, Guid subjectId, Guid classId, Guid academicSessionId, CancellationToken cancellationToken)
        => dbContext.Syllabi
            .Include(x => x.Subject)
            .Include(x => x.Class)
            .Include(x => x.AcademicSession)
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.SubjectId == subjectId && x.ClassId == classId && x.AcademicSessionId == academicSessionId, cancellationToken);

    public Task AddSyllabusAsync(Syllabus syllabus, CancellationToken cancellationToken)
        => dbContext.Syllabi.AddAsync(syllabus, cancellationToken).AsTask();

    public Task AddStudyMaterialAsync(StudyMaterial material, CancellationToken cancellationToken)
        => dbContext.StudyMaterials.AddAsync(material, cancellationToken).AsTask();

    public Task AddHomeworkAssignmentAsync(HomeworkAssignment homework, CancellationToken cancellationToken)
        => dbContext.HomeworkAssignments.AddAsync(homework, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<StudyMaterial>> GetStudyMaterialsAsync(Guid schoolId, Guid? classId, Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = dbContext.StudyMaterials
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .Where(x => x.SchoolId == schoolId);

        if (subjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == subjectId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(x => dbContext.Syllabi.Any(s => s.SchoolId == schoolId && s.ClassId == classId.Value && s.SubjectId == x.SubjectId));
        }

        return await query.OrderByDescending(x => x.UploadDateUtc).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HomeworkAssignment>> GetHomeworkAssignmentsAsync(Guid schoolId, Guid classId, Guid? sectionId, CancellationToken cancellationToken)
    {
        var query = dbContext.HomeworkAssignments
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Where(x => x.SchoolId == schoolId && x.ClassId == classId);

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        return await query.OrderBy(x => x.DueDateUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
