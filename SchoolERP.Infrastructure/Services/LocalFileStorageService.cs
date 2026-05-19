using SchoolERP.Application.Common.FileStorage;

namespace SchoolERP.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    public async Task<FileUploadResult> UploadAsync(Guid tenantId, string module, string category, FileUploadPayload payload, CancellationToken cancellationToken)
    {
        var safeExtension = Path.GetExtension(payload.OriginalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var relativePath = Path.Combine("uploads", tenantId.ToString("N"), module, category, storedFileName);
        var absolutePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", relativePath);
        var directoryPath = Path.GetDirectoryName(absolutePath) ?? throw new InvalidOperationException("Upload directory could not be resolved.");

        Directory.CreateDirectory(directoryPath);
        await File.WriteAllBytesAsync(absolutePath, payload.Content, cancellationToken);

        return new FileUploadResult
        {
            OriginalFileName = payload.OriginalFileName,
            StoredFileName = storedFileName,
            ContentType = payload.ContentType,
            FileSize = payload.Length,
            FileUrl = "/" + relativePath.Replace("\\", "/")
        };
    }
}
