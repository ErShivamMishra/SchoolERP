using SchoolERP.Application.Common.Exceptions;

namespace SchoolERP.Application.Common.FileStorage;

public static class FileUploadValidationHelper
{
    public static void Validate(FileUploadPayload? payload, IReadOnlyCollection<string> allowedContentTypes, long maxFileSizeInBytes, string fieldName)
    {
        if (payload is null || payload.Content.Length == 0)
        {
            throw new BadRequestException($"{fieldName} file is required.", "file_required");
        }

        if (payload.Length > maxFileSizeInBytes)
        {
            throw new BadRequestException($"{fieldName} file exceeds the maximum allowed size.", "file_size_exceeded");
        }

        if (string.IsNullOrWhiteSpace(payload.ContentType) || !allowedContentTypes.Contains(payload.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException($"{fieldName} file type is not allowed.", "file_type_not_allowed");
        }
    }
}
