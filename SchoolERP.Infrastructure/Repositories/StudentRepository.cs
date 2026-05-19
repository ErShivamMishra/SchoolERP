using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Students.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudentRepository(SchoolErpDbContext dbContext) : IStudentRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Admission?> GetAdmissionByIdAsync(Guid admissionId, CancellationToken cancellationToken)
        => dbContext.Admissions.Include(x => x.Student).FirstOrDefaultAsync(x => x.Id == admissionId, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.AcademicSession)
            .Include(x => x.AcademicInfo)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

    public async Task<(IReadOnlyCollection<Student> Items, int TotalCount)> GetStudentPageAsync(Guid schoolId, string? search, Guid? classId, Guid? sectionId, Guid? academicSessionId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Students
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.AcademicSession)
            .Include(x => x.AcademicInfo)
            .Include(x => x.Documents)
            .Where(x => x.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.RollNumber.Contains(term) ||
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term));
        }

        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        if (academicSessionId.HasValue)
        {
            query = query.Where(x => x.AcademicSessionId == academicSessionId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.FirstOrDefaultAsync(x => x.Id == academicSessionId, cancellationToken);

    public Task<bool> RollNumberExistsAsync(Guid schoolId, Guid classId, Guid sectionId, Guid academicSessionId, string rollNumber, Guid? excludeStudentId, CancellationToken cancellationToken)
        => dbContext.Students.AnyAsync(x =>
            x.SchoolId == schoolId &&
            x.ClassId == classId &&
            x.SectionId == sectionId &&
            x.AcademicSessionId == academicSessionId &&
            x.RollNumber == rollNumber &&
            (!excludeStudentId.HasValue || x.Id != excludeStudentId.Value), cancellationToken);

    public Task<bool> StudentEmailExistsAsync(Guid schoolId, string email, Guid? excludeStudentId, CancellationToken cancellationToken)
        => dbContext.Students.AnyAsync(x => x.SchoolId == schoolId && x.Email == email && (!excludeStudentId.HasValue || x.Id != excludeStudentId.Value), cancellationToken);

    public Task<bool> StudentMobileExistsAsync(Guid schoolId, string mobileNumber, Guid? excludeStudentId, CancellationToken cancellationToken)
        => dbContext.Students.AnyAsync(x => x.SchoolId == schoolId && x.MobileNumber == mobileNumber && (!excludeStudentId.HasValue || x.Id != excludeStudentId.Value), cancellationToken);

    public Task AddStudentAsync(Student student, CancellationToken cancellationToken)
        => dbContext.Students.AddAsync(student, cancellationToken).AsTask();

    public Task AddStudentAcademicInfoAsync(StudentAcademicInfo academicInfo, CancellationToken cancellationToken)
        => dbContext.StudentAcademicInfos.AddAsync(academicInfo, cancellationToken).AsTask();

    public Task AddStudentDocumentAsync(StudentDocument document, CancellationToken cancellationToken)
        => dbContext.StudentDocuments.AddAsync(document, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
