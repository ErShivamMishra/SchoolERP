using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.ParentPortal.Interfaces;

public interface IParentPortalRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Role?> GetRoleByCodeAsync(Guid schoolId, string roleCode, CancellationToken cancellationToken);
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);
    Task<Permission?> GetPermissionByCodeAsync(string permissionCode, CancellationToken cancellationToken);
    Task<bool> HasRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
    Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken);
    Task<bool> ParentEmailExistsAsync(Guid schoolId, string email, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task AddParentAsync(Parent parent, CancellationToken cancellationToken);
    Task<Parent?> GetParentByIdAsync(Guid parentId, CancellationToken cancellationToken);
    Task<Parent?> GetParentByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<ParentStudentRelation?> GetParentStudentRelationAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken);
    Task AddParentStudentRelationAsync(ParentStudentRelation relation, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ParentStudentRelation>> GetStudentRelationsAsync(Guid parentId, CancellationToken cancellationToken);
    Task<StudentAttendanceSummary?> GetAttendanceSummaryAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Invoice>> GetInvoicesAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExamResult>> GetExamResultsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<HomeworkAssignment>> GetHomeworkAsync(Guid schoolId, Guid classId, Guid sectionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NoticeBoardItem>> GetPublishedNoticesAsync(Guid schoolId, Guid classId, Guid sectionId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
