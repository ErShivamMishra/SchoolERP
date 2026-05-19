using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Teachers.Interfaces;

public interface ITeacherRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Teacher> Items, int TotalCount)> GetTeacherPageAsync(Guid schoolId, string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<bool> EmployeeCodeExistsAsync(Guid schoolId, string employeeCode, Guid? excludeTeacherId, CancellationToken cancellationToken);
    Task<bool> TeacherEmailExistsAsync(Guid schoolId, string email, Guid? excludeTeacherId, CancellationToken cancellationToken);
    Task AddTeacherAsync(Teacher teacher, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Subject>> GetSubjectsByIdsAsync(Guid schoolId, IReadOnlyCollection<Guid> subjectIds, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task RemoveTeacherSubjectsAsync(IEnumerable<TeacherSubject> subjects);
    Task AddTeacherSubjectsAsync(IEnumerable<TeacherSubject> subjects, CancellationToken cancellationToken);
    Task RemoveTeacherClassAssignmentsAsync(IEnumerable<TeacherClassAssignment> assignments);
    Task AddTeacherClassAssignmentsAsync(IEnumerable<TeacherClassAssignment> assignments, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
