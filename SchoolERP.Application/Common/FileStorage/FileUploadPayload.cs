namespace SchoolERP.Application.Common.FileStorage;

public sealed class FileUploadPayload
{
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public long Length => Content.LongLength;
}
