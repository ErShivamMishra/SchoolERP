using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Options;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Seeding;

public static class SchoolErpDbInitializer
{
    private const string InitialMigrationId = "20260512085003_AddSchoolManagementModule";

    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SchoolErpDbInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolErpDbContext>();
        var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;

        await EnsureLegacyMigrationHistoryAsync(dbContext, cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
        await SeedModulesAsync(dbContext, cancellationToken);
        await SeedPermissionsAsync(dbContext, cancellationToken);
        await SeedSubscriptionPlansAsync(dbContext, cancellationToken);
        await SeedPlanEntitlementsAsync(dbContext, cancellationToken);
        await SeedRolesAsync(dbContext, cancellationToken);
        await SeedSuperAdminAsync(dbContext, authOptions, logger, cancellationToken);
    }

    private static async Task SeedModulesAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var seedModules = new[]
        {
            new { Name = "Admission Management", Code = ModuleCodes.AdmissionManagement, Description = "Admissions, approvals, and applicant intake.", DisplayOrder = 5 },
            new { Name = "School Management", Code = ModuleCodes.SchoolManagement, Description = "Platform school onboarding and lifecycle management.", DisplayOrder = 10 },
            new { Name = "Staff Management", Code = ModuleCodes.StaffManagement, Description = "Tenant staff administration and permissions.", DisplayOrder = 20 },
            new { Name = "Student Management", Code = ModuleCodes.StudentManagement, Description = "Student records and profiles.", DisplayOrder = 30 },
            new { Name = "Teacher Management", Code = ModuleCodes.TeacherManagement, Description = "Teacher management operations.", DisplayOrder = 40 },
            new { Name = "Attendance Management", Code = ModuleCodes.AttendanceManagement, Description = "Attendance workflows.", DisplayOrder = 50 },
            new { Name = "Fee Management", Code = ModuleCodes.FeeManagement, Description = "Fee setup and collection.", DisplayOrder = 60 },
            new { Name = "Result Management", Code = ModuleCodes.ResultManagement, Description = "Results and grading.", DisplayOrder = 70 },
            new { Name = "Quiz Management", Code = ModuleCodes.QuizManagement, Description = "Quiz and assessment workflows.", DisplayOrder = 80 },
            new { Name = "Study Management", Code = ModuleCodes.StudyManagement, Description = "Study materials and content.", DisplayOrder = 90 },
            new { Name = "Dashboard Management", Code = ModuleCodes.DashboardManagement, Description = "Operational dashboards, analytics, audits, and reporting.", DisplayOrder = 100 },
            new { Name = "Notice Board Management", Code = ModuleCodes.NoticeBoardManagement, Description = "Announcements, notices, and audience targeting.", DisplayOrder = 110 },
            new { Name = "Communication Management", Code = ModuleCodes.CommunicationManagement, Description = "Parent-teacher communication and conversation history.", DisplayOrder = 120 },
            new { Name = "Transport Management", Code = ModuleCodes.TransportManagement, Description = "Vehicles, routes, drivers, and student assignments.", DisplayOrder = 130 },
            new { Name = "Gallery Management", Code = ModuleCodes.GalleryManagement, Description = "Photo and video album management.", DisplayOrder = 140 },
            new { Name = "ID Card Management", Code = ModuleCodes.IdCardManagement, Description = "ID card issuance.", DisplayOrder = 150 },
            new { Name = "Admit Card Management", Code = ModuleCodes.AdmitCardManagement, Description = "Admit card generation.", DisplayOrder = 160 },
            new { Name = "Parent Portal Management", Code = ModuleCodes.ParentPortalManagement, Description = "Parent accounts, student linkage, and parent self-service APIs.", DisplayOrder = 170 }
        };

        foreach (var seedModule in seedModules)
        {
            var module = await dbContext.Modules.FirstOrDefaultAsync(x => x.Code == seedModule.Code, cancellationToken);
            if (module is null)
            {
                await dbContext.Modules.AddAsync(new Module
                {
                    Name = seedModule.Name,
                    Code = seedModule.Code,
                    Description = seedModule.Description,
                    DisplayOrder = seedModule.DisplayOrder,
                    IsActive = true
                }, cancellationToken);
                continue;
            }

            module.Name = seedModule.Name;
            module.Description = seedModule.Description;
            module.DisplayOrder = seedModule.DisplayOrder;
            module.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPermissionsAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var modules = await dbContext.Modules.ToListAsync(cancellationToken);
        foreach (var module in modules)
        {
            foreach (var action in PermissionActions.All)
            {
                var permissionCode = $"{module.Code}.{action}";
                var permission = await dbContext.Permissions.FirstOrDefaultAsync(x => x.Code == permissionCode, cancellationToken);
                if (permission is null)
                {
                    await dbContext.Permissions.AddAsync(new Permission
                    {
                        ModuleId = module.Id,
                        Name = $"{module.Name} {action}",
                        Code = permissionCode
                    }, cancellationToken);
                    continue;
                }

                permission.ModuleId = module.Id;
                permission.Name = $"{module.Name} {action}";
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSubscriptionPlansAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.SubscriptionPlans.AnyAsync(x => x.Code == "BASIC", cancellationToken))
        {
            await dbContext.SubscriptionPlans.AddAsync(new SubscriptionPlan
            {
                Name = "Basic",
                Code = "BASIC",
                Price = 0,
                IsActive = true
            }, cancellationToken);
        }

        if (!await dbContext.SubscriptionPlans.AnyAsync(x => x.Code == "PREMIUM", cancellationToken))
        {
            await dbContext.SubscriptionPlans.AddAsync(new SubscriptionPlan
            {
                Name = "Premium",
                Code = "PREMIUM",
                Price = 0,
                IsActive = true
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPlanEntitlementsAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var modules = await dbContext.Modules.ToListAsync(cancellationToken);
        var basicPlan = await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Code == "BASIC", cancellationToken);
        var premiumPlan = await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Code == "PREMIUM", cancellationToken);

        if (basicPlan is not null)
        {
            var basicCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ModuleCodes.AdmissionManagement,
                ModuleCodes.StaffManagement,
                ModuleCodes.StudentManagement,
                ModuleCodes.TeacherManagement,
                ModuleCodes.AttendanceManagement,
                ModuleCodes.StudyManagement,
                ModuleCodes.DashboardManagement,
                ModuleCodes.ResultManagement,
                ModuleCodes.NoticeBoardManagement,
                ModuleCodes.ParentPortalManagement
            };

            await UpsertPlanEntitlementsAsync(dbContext, basicPlan.Id, modules, basicCodes, cancellationToken);
        }

        if (premiumPlan is not null)
        {
            await UpsertPlanEntitlementsAsync(dbContext, premiumPlan.Id, modules, modules.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        foreach (var roleName in RoleNames.All)
        {
            var exists = await dbContext.Roles.AnyAsync(x => x.Code == roleName && x.TenantId == null, cancellationToken);
            if (exists)
            {
                continue;
            }

            await dbContext.Roles.AddAsync(new Role
            {
                Name = roleName,
                Code = roleName,
                IsSystemRole = true
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSuperAdminAsync(SchoolErpDbContext dbContext, AuthOptions authOptions, ILogger logger, CancellationToken cancellationToken)
    {
        var seedUser = authOptions.SeedSuperAdmin;
        var normalizedEmail = seedUser.Email.Trim().ToUpperInvariant();
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        var superAdminRole = await dbContext.Roles.FirstAsync(x => x.Code == RoleNames.SuperAdmin && x.TenantId == null, cancellationToken);

        await dbContext.Users.AddAsync(new User
        {
            FullName = seedUser.FullName.Trim(),
            Email = seedUser.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedUser.Password),
            RoleId = superAdminRole.Id,
            IsActive = true,
            IsPlatformUser = true,
            RequiresPasswordReset = false,
            FailedLoginAttempts = 0
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded SuperAdmin user with email {Email}", seedUser.Email.Trim());
    }

    private static async Task EnsureLegacyMigrationHistoryAsync(SchoolErpDbContext dbContext, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF OBJECT_ID(N'Modules', N'U') IS NOT NULL
            BEGIN
                IF OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{InitialMigrationId}')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('{InitialMigrationId}', '8.0.8');
                END
            END
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }

    private static async Task UpsertPlanEntitlementsAsync(
        SchoolErpDbContext dbContext,
        Guid planId,
        IReadOnlyCollection<Module> modules,
        ISet<string> licensedModuleCodes,
        CancellationToken cancellationToken)
    {
        foreach (var module in modules)
        {
            var entitlement = await dbContext.PlanModuleEntitlements
                .FirstOrDefaultAsync(x => x.SubscriptionPlanId == planId && x.ModuleId == module.Id, cancellationToken);

            if (entitlement is null)
            {
                await dbContext.PlanModuleEntitlements.AddAsync(new PlanModuleEntitlement
                {
                    SubscriptionPlanId = planId,
                    ModuleId = module.Id,
                    IsLicensed = licensedModuleCodes.Contains(module.Code),
                    IsVisibleInMenu = licensedModuleCodes.Contains(module.Code)
                }, cancellationToken);
                continue;
            }

            entitlement.IsLicensed = licensedModuleCodes.Contains(module.Code);
            entitlement.IsVisibleInMenu = licensedModuleCodes.Contains(module.Code);
        }
    }
}
