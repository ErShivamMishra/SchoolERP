using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Admissions.Interfaces;

public interface IAdmissionRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<bool> AcademicSessionNameExistsAsync(Guid schoolId, string name, CancellationToken cancellationToken);
    Task<bool> ClassNameExistsAsync(Guid schoolId, string name, CancellationToken cancellationToken);
    Task<bool> SectionNameExistsAsync(Guid classId, string name, CancellationToken cancellationToken);
    Task AddAcademicSessionAsync(AcademicSession session, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AcademicSession>> GetAcademicSessionsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddClassAsync(Class entity, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Class>> GetClassesAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddSectionAsync(Section entity, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Section>> GetSectionsAsync(Guid schoolId, Guid? classId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken);
    Task<bool> AdmissionNumberExistsAsync(Guid schoolId, string admissionNumber, Guid? excludeAdmissionId, CancellationToken cancellationToken);
    Task<bool> MobileExistsAsync(Guid schoolId, string mobileNumber, Guid? excludeAdmissionId, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(Guid schoolId, string email, Guid? excludeAdmissionId, CancellationToken cancellationToken);
    Task AddAdmissionAsync(Admission admission, CancellationToken cancellationToken);
    Task<Admission?> GetAdmissionByIdAsync(Guid admissionId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Admission> Items, int TotalCount)> GetAdmissionPageAsync(Guid schoolId, string? search, AdmissionStatus? status, Guid? appliedClassId, Guid? academicSessionId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
