using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Study.Interfaces;

public interface IStudyRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<bool> SubjectCodeExistsAsync(Guid schoolId, string code, Guid? excludeSubjectId, CancellationToken cancellationToken);
    Task AddSubjectAsync(Subject subject, CancellationToken cancellationToken);
    Task<Subject?> GetSubjectByIdAsync(Guid subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Subject>> GetSubjectsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<Syllabus?> GetSyllabusAsync(Guid schoolId, Guid subjectId, Guid classId, Guid academicSessionId, CancellationToken cancellationToken);
    Task AddSyllabusAsync(Syllabus syllabus, CancellationToken cancellationToken);
    Task AddStudyMaterialAsync(StudyMaterial material, CancellationToken cancellationToken);
    Task AddHomeworkAssignmentAsync(HomeworkAssignment homework, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudyMaterial>> GetStudyMaterialsAsync(Guid schoolId, Guid? classId, Guid? subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<HomeworkAssignment>> GetHomeworkAssignmentsAsync(Guid schoolId, Guid classId, Guid? sectionId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
