using SchoolERP.Application.Features.IdCards.Models;

namespace SchoolERP.Application.Features.IdCards.Interfaces;

public interface IIdCardService
{
    Task<IdCardTemplateDto> CreateTemplateAsync(CreateIdCardTemplateRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GeneratedIdCardDto>> GenerateStudentCardsAsync(GenerateStudentIdCardsRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GeneratedIdCardDto>> GenerateTeacherCardsAsync(GenerateTeacherIdCardsRequestDto request, CancellationToken cancellationToken);
}
