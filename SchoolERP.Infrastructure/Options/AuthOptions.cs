namespace SchoolERP.Infrastructure.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public int FailedLoginThreshold { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public SeedSuperAdminOptions SeedSuperAdmin { get; set; } = new();
}

public sealed class SeedSuperAdminOptions
{
    public string FullName { get; set; } = "Platform Super Admin";
    public string Email { get; set; } = "superadmin@schoolerp.local";
    public string Password { get; set; } = "SuperAdmin@123";
}
