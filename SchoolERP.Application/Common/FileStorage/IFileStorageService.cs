namespace SchoolERP.Application.Common.FileStorage;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(Guid tenantId, string module, string category, FileUploadPayload payload, CancellationToken cancellationToken);
}
