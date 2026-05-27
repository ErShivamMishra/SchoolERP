namespace SchoolERP.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SchoolAdmin = "SchoolAdmin";
    public const string Staff = "Staff";
    public const string Parent = "Parent";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        SuperAdmin,
        SchoolAdmin,
        Staff,
        Parent
    };
}
