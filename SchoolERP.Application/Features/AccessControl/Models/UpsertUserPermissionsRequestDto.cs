namespace SchoolERP.Application.Features.AccessControl.Models;

public sealed class UpsertUserPermissionsRequestDto
{
    public IReadOnlyCollection<UpsertUserModulePermissionDto> Permissions { get; init; } = Array.Empty<UpsertUserModulePermissionDto>();
}
