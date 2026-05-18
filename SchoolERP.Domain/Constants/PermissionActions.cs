namespace SchoolERP.Domain.Constants;

public static class PermissionActions
{
    public const string View = "View";
    public const string Create = "Create";
    public const string Edit = "Edit";
    public const string Delete = "Delete";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        View,
        Create,
        Edit,
        Delete
    };
}
