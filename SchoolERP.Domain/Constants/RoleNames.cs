namespace SchoolERP.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SchoolAdmin = "SchoolAdmin";
    public const string Staff = "Staff";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        SuperAdmin,
        SchoolAdmin,
        Staff
    };
}
