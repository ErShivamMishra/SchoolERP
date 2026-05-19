namespace SchoolERP.Application.Common.FileStorage;

public sealed class FileUploadResult
{
    public string OriginalFileName { get; init; } = string.Empty;
    public string StoredFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string FileUrl { get; init; } = string.Empty;
}
