using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Admissions.Models;

namespace SchoolERP.Application.Features.Admissions.Interfaces;

public interface IAdmissionService
{
    Task<AcademicSessionDto> CreateAcademicSessionAsync(CreateAcademicSessionRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AcademicSessionDto>> GetAcademicSessionsAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<ClassDto> CreateClassAsync(CreateClassRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ClassDto>> GetClassesAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<SectionDto> CreateSectionAsync(CreateSectionRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SectionDto>> GetSectionsAsync(Guid? schoolId, Guid? classId, CancellationToken cancellationToken);
    Task<AdmissionDto> CreateAsync(CreateAdmissionRequestDto request, CancellationToken cancellationToken);
    Task<AdmissionDto> UpdateAsync(Guid admissionId, UpdateAdmissionRequestDto request, CancellationToken cancellationToken);
    Task<AdmissionDto> ApproveAsync(Guid admissionId, ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken);
    Task<AdmissionDto> RejectAsync(Guid admissionId, ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken);
    Task<AdmissionDto> GetByIdAsync(Guid admissionId, CancellationToken cancellationToken);
    Task<PagedResult<AdmissionDto>> GetAllAsync(GetAdmissionsRequestDto request, CancellationToken cancellationToken);
}
