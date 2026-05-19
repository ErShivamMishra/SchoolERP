using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Teachers.Models;

namespace SchoolERP.Application.Features.Teachers.Interfaces;

public interface ITeacherService
{
    Task<TeacherDto> CreateAsync(CreateTeacherRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> UpdateAsync(Guid teacherId, UpdateTeacherRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> AssignSubjectsAsync(Guid teacherId, AssignTeacherSubjectsRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> AssignClassesAsync(Guid teacherId, AssignTeacherClassesRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> GetByIdAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<PagedResult<TeacherDto>> GetAllAsync(GetTeachersRequestDto request, CancellationToken cancellationToken);
    Task<TeacherDto> DeactivateAsync(Guid teacherId, DeactivateTeacherRequestDto request, CancellationToken cancellationToken);
}
