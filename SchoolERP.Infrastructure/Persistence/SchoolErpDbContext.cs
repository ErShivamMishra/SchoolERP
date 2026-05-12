using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Persistence;

public sealed class SchoolErpDbContext : DbContext, IUnitOfWork
{
    public SchoolErpDbContext(DbContextOptions<SchoolErpDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PlanModuleEntitlement> PlanModuleEntitlements => Set<PlanModuleEntitlement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolSubscription> SchoolSubscriptions => Set<SchoolSubscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Campus>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => new { x.TenantId, x.NormalizedEmail }).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        modelBuilder.Entity<Module>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Permission>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<SubscriptionPlan>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.Token).IsUnique();

        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<PlanModuleEntitlement>().HasKey(x => new { x.SubscriptionPlanId, x.ModuleId });

        modelBuilder.Entity<School>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Campus>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Module>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<SubscriptionPlan>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<SchoolSubscription>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Campuses)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Users)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Subscriptions)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Permission>()
            .HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasMany(x => x.Permissions)
            .WithOne(x => x.Module)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubscriptionPlan>()
            .HasMany(x => x.PlanModuleEntitlements)
            .WithOne(x => x.SubscriptionPlan)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasMany(x => x.PlanModuleEntitlements)
            .WithOne(x => x.Module)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubscriptionPlan>()
            .HasMany(x => x.SchoolSubscriptions)
            .WithOne(x => x.SubscriptionPlan)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
