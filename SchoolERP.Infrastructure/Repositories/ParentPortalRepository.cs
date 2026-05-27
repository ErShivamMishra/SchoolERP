using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.ParentPortal.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ParentPortalRepository(SchoolErpDbContext dbContext) : IParentPortalRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Role?> GetRoleByCodeAsync(Guid schoolId, string roleCode, CancellationToken cancellationToken)
        => dbContext.Roles.FirstOrDefaultAsync(x => x.TenantId == schoolId && x.Code == roleCode, cancellationToken);

    public Task AddRoleAsync(Role role, CancellationToken cancellationToken)
        => dbContext.Roles.AddAsync(role, cancellationToken).AsTask();

    public Task<Permission?> GetPermissionByCodeAsync(string permissionCode, CancellationToken cancellationToken)
        => dbContext.Permissions.FirstOrDefaultAsync(x => x.Code == permissionCode, cancellationToken);

    public Task<bool> HasRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
        => dbContext.RolePermissions.AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId, cancellationToken);

    public Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken)
        => dbContext.RolePermissions.AddAsync(rolePermission, cancellationToken).AsTask();

    public Task<bool> ParentEmailExistsAsync(Guid schoolId, string email, CancellationToken cancellationToken)
        => dbContext.Parents.AnyAsync(x => x.SchoolId == schoolId && x.Email.ToUpper() == email, cancellationToken);

    public Task AddUserAsync(User user, CancellationToken cancellationToken)
        => dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task AddParentAsync(Parent parent, CancellationToken cancellationToken)
        => dbContext.Parents.AddAsync(parent, cancellationToken).AsTask();

    public Task<Parent?> GetParentByIdAsync(Guid parentId, CancellationToken cancellationToken)
        => dbContext.Parents.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken);

    public Task<Parent?> GetParentByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.Parents.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students
            .Include(x => x.Class)
            .Include(x => x.Section)
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.Id == studentId, cancellationToken);

    public Task<ParentStudentRelation?> GetParentStudentRelationAsync(Guid parentId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.ParentStudentRelations
            .Include(x => x.Student)!.ThenInclude(x => x!.Class)
            .Include(x => x.Student)!.ThenInclude(x => x!.Section)
            .FirstOrDefaultAsync(x => x.ParentId == parentId && x.StudentId == studentId, cancellationToken);

    public Task AddParentStudentRelationAsync(ParentStudentRelation relation, CancellationToken cancellationToken)
        => dbContext.ParentStudentRelations.AddAsync(relation, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<ParentStudentRelation>> GetStudentRelationsAsync(Guid parentId, CancellationToken cancellationToken)
        => await dbContext.ParentStudentRelations
            .Include(x => x.Student)!.ThenInclude(x => x!.Class)
            .Include(x => x.Student)!.ThenInclude(x => x!.Section)
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.Student!.RollNumber)
            .ToListAsync(cancellationToken);

    public Task<StudentAttendanceSummary?> GetAttendanceSummaryAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.StudentAttendanceSummaries.FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.StudentId == studentId, cancellationToken);

    public async Task<IReadOnlyCollection<Invoice>> GetInvoicesAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => await dbContext.Invoices
            .AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ExamResult>> GetExamResultsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => await dbContext.ExamResults
            .AsNoTracking()
            .Include(x => x.Exam)
            .Include(x => x.ExamSubject)
            .Where(x => x.SchoolId == schoolId && x.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<HomeworkAssignment>> GetHomeworkAsync(Guid schoolId, Guid classId, Guid sectionId, CancellationToken cancellationToken)
        => await dbContext.HomeworkAssignments
            .AsNoTracking()
            .Include(x => x.Subject)
            .Where(x => x.SchoolId == schoolId && x.ClassId == classId && x.SectionId == sectionId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<NoticeBoardItem>> GetPublishedNoticesAsync(Guid schoolId, Guid classId, Guid sectionId, CancellationToken cancellationToken)
        => await dbContext.NoticeBoardItems
            .AsNoTracking()
            .Where(x =>
                x.SchoolId == schoolId &&
                x.IsPublished &&
                (!x.ExpiryDateUtc.HasValue || x.ExpiryDateUtc >= DateTime.UtcNow) &&
                (x.AudienceType == NoticeAudienceType.All ||
                 x.AudienceType == NoticeAudienceType.Students ||
                 x.AudienceType == NoticeAudienceType.Parents ||
                 (x.ClassId == classId && (!x.SectionId.HasValue || x.SectionId == sectionId))))
            .OrderByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
