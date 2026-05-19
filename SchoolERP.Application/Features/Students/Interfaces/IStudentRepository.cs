using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Students.Interfaces;

public interface IStudentRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Admission?> GetAdmissionByIdAsync(Guid admissionId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Student> Items, int TotalCount)> GetStudentPageAsync(Guid schoolId, string? search, Guid? classId, Guid? sectionId, Guid? academicSessionId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken);
    Task<bool> RollNumberExistsAsync(Guid schoolId, Guid classId, Guid sectionId, Guid academicSessionId, string rollNumber, Guid? excludeStudentId, CancellationToken cancellationToken);
    Task<bool> StudentEmailExistsAsync(Guid schoolId, string email, Guid? excludeStudentId, CancellationToken cancellationToken);
    Task<bool> StudentMobileExistsAsync(Guid schoolId, string mobileNumber, Guid? excludeStudentId, CancellationToken cancellationToken);
    Task AddStudentAsync(Student student, CancellationToken cancellationToken);
    Task AddStudentAcademicInfoAsync(StudentAcademicInfo academicInfo, CancellationToken cancellationToken);
    Task AddStudentDocumentAsync(StudentDocument document, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
