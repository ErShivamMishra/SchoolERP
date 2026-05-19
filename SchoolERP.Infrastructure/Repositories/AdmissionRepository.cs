using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Admissions.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AdmissionRepository(SchoolErpDbContext dbContext) : IAdmissionRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<bool> AcademicSessionNameExistsAsync(Guid schoolId, string name, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.AnyAsync(x => x.TenantId == schoolId && x.Name == name, cancellationToken);

    public Task<bool> ClassNameExistsAsync(Guid schoolId, string name, CancellationToken cancellationToken)
        => dbContext.Classes.AnyAsync(x => x.TenantId == schoolId && x.Name == name, cancellationToken);

    public Task<bool> SectionNameExistsAsync(Guid classId, string name, CancellationToken cancellationToken)
        => dbContext.Sections.AnyAsync(x => x.ClassId == classId && x.Name == name, cancellationToken);

    public Task AddAcademicSessionAsync(AcademicSession session, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.AddAsync(session, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<AcademicSession>> GetAcademicSessionsAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.AcademicSessions.Where(x => x.TenantId == schoolId).OrderByDescending(x => x.StartDateUtc).ToListAsync(cancellationToken);

    public Task AddClassAsync(Class entity, CancellationToken cancellationToken)
        => dbContext.Classes.AddAsync(entity, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Class>> GetClassesAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.Classes.Where(x => x.TenantId == schoolId).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task AddSectionAsync(Section entity, CancellationToken cancellationToken)
        => dbContext.Sections.AddAsync(entity, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Section>> GetSectionsAsync(Guid schoolId, Guid? classId, CancellationToken cancellationToken)
    {
        var query = dbContext.Sections.Include(x => x.Class).Where(x => x.TenantId == schoolId);
        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        return await query.OrderBy(x => x.Class!.Name).ThenBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.FirstOrDefaultAsync(x => x.Id == academicSessionId, cancellationToken);

    public Task<bool> AdmissionNumberExistsAsync(Guid schoolId, string admissionNumber, Guid? excludeAdmissionId, CancellationToken cancellationToken)
        => dbContext.Admissions.AnyAsync(x => x.SchoolId == schoolId && x.AdmissionNumber == admissionNumber && (!excludeAdmissionId.HasValue || x.Id != excludeAdmissionId.Value), cancellationToken);

    public async Task<bool> MobileExistsAsync(Guid schoolId, string mobileNumber, Guid? excludeAdmissionId, CancellationToken cancellationToken)
    {
        if (await dbContext.Admissions.AnyAsync(x => x.SchoolId == schoolId && x.MobileNumber == mobileNumber && (!excludeAdmissionId.HasValue || x.Id != excludeAdmissionId.Value), cancellationToken))
        {
            return true;
        }

        return await dbContext.Students.AnyAsync(x => x.SchoolId == schoolId && x.MobileNumber == mobileNumber, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Guid schoolId, string email, Guid? excludeAdmissionId, CancellationToken cancellationToken)
    {
        if (await dbContext.Admissions.AnyAsync(x => x.SchoolId == schoolId && x.Email == email && (!excludeAdmissionId.HasValue || x.Id != excludeAdmissionId.Value), cancellationToken))
        {
            return true;
        }

        return await dbContext.Students.AnyAsync(x => x.SchoolId == schoolId && x.Email == email, cancellationToken);
    }

    public Task AddAdmissionAsync(Admission admission, CancellationToken cancellationToken)
        => dbContext.Admissions.AddAsync(admission, cancellationToken).AsTask();

    public Task<Admission?> GetAdmissionByIdAsync(Guid admissionId, CancellationToken cancellationToken)
        => dbContext.Admissions
            .Include(x => x.AppliedClass)
            .Include(x => x.AcademicSession)
            .Include(x => x.GuardianDetails)
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == admissionId, cancellationToken);

    public async Task<(IReadOnlyCollection<Admission> Items, int TotalCount)> GetAdmissionPageAsync(Guid schoolId, string? search, AdmissionStatus? status, Guid? appliedClassId, Guid? academicSessionId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Admissions
            .Include(x => x.AppliedClass)
            .Include(x => x.AcademicSession)
            .Include(x => x.Student)
            .Where(x => x.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.AdmissionNumber.Contains(term) ||
                x.StudentFirstName.Contains(term) ||
                x.StudentLastName.Contains(term) ||
                x.MobileNumber.Contains(term) ||
                (x.Email != null && x.Email.Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (appliedClassId.HasValue)
        {
            query = query.Where(x => x.AppliedClassId == appliedClassId.Value);
        }

        if (academicSessionId.HasValue)
        {
            query = query.Where(x => x.AcademicSessionId == academicSessionId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
