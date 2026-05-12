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
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SchoolErpDbInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolErpDbContext>();
        var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await SeedSubscriptionPlansAsync(dbContext, cancellationToken);
        await SeedRolesAsync(dbContext, cancellationToken);
        await SeedSuperAdminAsync(dbContext, authOptions, logger, cancellationToken);
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
}
