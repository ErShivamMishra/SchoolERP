using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Students.Models;

namespace SchoolERP.Application.Features.Students.Interfaces;

public interface IStudentService
{
    Task<StudentDto> ConvertAdmissionAsync(ConvertAdmissionToStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> CreateAsync(CreateStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> UpdateAsync(Guid studentId, UpdateStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> GetByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<PagedResult<StudentDto>> GetAllAsync(GetStudentsRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> PromoteAsync(Guid studentId, PromoteStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> TransferAsync(Guid studentId, TransferStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDto> DeactivateAsync(Guid studentId, DeactivateStudentRequestDto request, CancellationToken cancellationToken);
    Task<StudentDocumentDto> UploadDocumentAsync(Guid studentId, UploadStudentDocumentRequestDto request, CancellationToken cancellationToken);
}
