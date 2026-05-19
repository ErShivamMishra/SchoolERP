using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Teachers.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class TeacherRepository(SchoolErpDbContext dbContext) : ITeacherRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Teacher?> GetTeacherByIdAsync(Guid teacherId, CancellationToken cancellationToken)
        => dbContext.Teachers
            .Include(x => x.Subjects).ThenInclude(x => x.Subject)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.Class)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.Section)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.AcademicSession)
            .FirstOrDefaultAsync(x => x.Id == teacherId, cancellationToken);

    public async Task<(IReadOnlyCollection<Teacher> Items, int TotalCount)> GetTeacherPageAsync(Guid schoolId, string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Teachers
            .Include(x => x.Subjects).ThenInclude(x => x.Subject)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.Class)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.Section)
            .Include(x => x.ClassAssignments).ThenInclude(x => x.AcademicSession)
            .Where(x => x.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.EmployeeCode.Contains(term) ||
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term) ||
                x.MobileNumber.Contains(term) ||
                (x.Email != null && x.Email.Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<bool> EmployeeCodeExistsAsync(Guid schoolId, string employeeCode, Guid? excludeTeacherId, CancellationToken cancellationToken)
        => dbContext.Teachers.AnyAsync(x => x.SchoolId == schoolId && x.EmployeeCode == employeeCode && (!excludeTeacherId.HasValue || x.Id != excludeTeacherId.Value), cancellationToken);

    public Task<bool> TeacherEmailExistsAsync(Guid schoolId, string email, Guid? excludeTeacherId, CancellationToken cancellationToken)
        => dbContext.Teachers.AnyAsync(x => x.SchoolId == schoolId && x.Email == email && (!excludeTeacherId.HasValue || x.Id != excludeTeacherId.Value), cancellationToken);

    public Task AddTeacherAsync(Teacher teacher, CancellationToken cancellationToken)
        => dbContext.Teachers.AddAsync(teacher, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Subject>> GetSubjectsByIdsAsync(Guid schoolId, IReadOnlyCollection<Guid> subjectIds, CancellationToken cancellationToken)
        => await dbContext.Subjects.Where(x => x.SchoolId == schoolId && subjectIds.Contains(x.Id)).ToListAsync(cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public void RemoveTeacherSubjectsAsync(IEnumerable<TeacherSubject> subjects)
        => dbContext.TeacherSubjects.RemoveRange(subjects);

    Task ITeacherRepository.RemoveTeacherSubjectsAsync(IEnumerable<TeacherSubject> subjects)
    {
        dbContext.TeacherSubjects.RemoveRange(subjects);
        return Task.CompletedTask;
    }

    public Task AddTeacherSubjectsAsync(IEnumerable<TeacherSubject> subjects, CancellationToken cancellationToken)
        => dbContext.TeacherSubjects.AddRangeAsync(subjects, cancellationToken);

    public void RemoveTeacherClassAssignmentsAsync(IEnumerable<TeacherClassAssignment> assignments)
        => dbContext.TeacherClassAssignments.RemoveRange(assignments);

    Task ITeacherRepository.RemoveTeacherClassAssignmentsAsync(IEnumerable<TeacherClassAssignment> assignments)
    {
        dbContext.TeacherClassAssignments.RemoveRange(assignments);
        return Task.CompletedTask;
    }

    public Task AddTeacherClassAssignmentsAsync(IEnumerable<TeacherClassAssignment> assignments, CancellationToken cancellationToken)
        => dbContext.TeacherClassAssignments.AddRangeAsync(assignments, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
