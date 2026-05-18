namespace SchoolERP.Application.Features.Modules.Models;

public sealed class ModuleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
}
