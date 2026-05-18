namespace SchoolERP.API.Common.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class ModuleAccessAttribute(string moduleCode, string permissionAction, string? authorizationNote = null) : Attribute
{
    public string ModuleCode { get; } = moduleCode;
    public string PermissionAction { get; } = permissionAction;
    public string? AuthorizationNote { get; } = authorizationNote;
}
