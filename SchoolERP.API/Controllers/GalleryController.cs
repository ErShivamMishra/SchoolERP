using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Requests;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Gallery.Interfaces;
using SchoolERP.Application.Features.Gallery.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Enums;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/gallery")]
public sealed class GalleryController(IGalleryService galleryService) : ControllerBase
{
    [HttpPost("albums")]
    [ModuleAccess(ModuleCodes.GalleryManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await galleryService.CreateAlbumAsync(request, cancellationToken), "Gallery album created successfully."));

    [HttpPatch("albums/{albumId:guid}/publish")]
    [ModuleAccess(ModuleCodes.GalleryManagement, PermissionActions.Edit)]
    public async Task<IActionResult> PublishAlbum(Guid albumId, [FromBody] PublishAlbumRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await galleryService.PublishAlbumAsync(albumId, request, cancellationToken), "Gallery album publication status updated successfully."));

    [HttpPost("albums/{albumId:guid}/media")]
    [ModuleAccess(ModuleCodes.GalleryManagement, PermissionActions.Create)]
    public async Task<IActionResult> UploadMedia(Guid albumId, [FromForm] UploadGalleryMediaFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await galleryService.UploadMediaAsync(albumId, new UploadGalleryMediaRequestDto
        {
            SchoolId = request.SchoolId,
            Title = request.Title,
            Description = request.Description,
            MediaType = (MediaType)request.MediaType,
            File = await ToPayloadAsync(request.File, cancellationToken)
        }, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Gallery media uploaded successfully."));
    }

    [HttpGet("albums")]
    [ModuleAccess(ModuleCodes.GalleryManagement, PermissionActions.View)]
    public async Task<IActionResult> GetAlbums([FromQuery] GetAlbumListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await galleryService.GetAlbumsAsync(request, cancellationToken), "Gallery albums fetched successfully."));

    [HttpGet("albums/{albumId:guid}/media")]
    [ModuleAccess(ModuleCodes.GalleryManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMedia(Guid albumId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await galleryService.GetMediaAsync(albumId, cancellationToken), "Gallery media fetched successfully."));

    private static async Task<FileUploadPayload?> ToPayloadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return null;
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return new FileUploadPayload
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Content = memory.ToArray()
        };
    }
}
