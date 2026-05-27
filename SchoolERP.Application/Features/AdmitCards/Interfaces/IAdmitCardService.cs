using SchoolERP.Application.Features.AdmitCards.Models;

namespace SchoolERP.Application.Features.AdmitCards.Interfaces;

public interface IAdmitCardService
{
    Task<AdmitCardTemplateDto> CreateTemplateAsync(CreateAdmitCardTemplateRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GeneratedAdmitCardDto>> GenerateAsync(GenerateAdmitCardsRequestDto request, CancellationToken cancellationToken);
}
