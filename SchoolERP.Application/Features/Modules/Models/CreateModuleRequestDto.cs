namespace SchoolERP.Application.Features.Modules.Models;

public sealed class CreateModuleRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
}
