using System.Text.Json;
using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AdmitCards.Interfaces;
using SchoolERP.Application.Features.AdmitCards.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.AdmitCards.Services;

public sealed class AdmitCardService(
    IAdmitCardRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateAdmitCardTemplateRequestDto> createValidator,
    IValidator<GenerateAdmitCardsRequestDto> generateValidator) : IAdmitCardService
{
    public async Task<AdmitCardTemplateDto> CreateTemplateAsync(CreateAdmitCardTemplateRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var version = await repository.GetNextTemplateVersionAsync(schoolId, request.TemplateName.Trim(), cancellationToken);

        var template = new AdmitCardTemplate
        {
            SchoolId = schoolId,
            TemplateName = request.TemplateName.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            SchoolDetails = request.SchoolDetails.Trim(),
            LayoutJson = request.LayoutJson,
            Version = version,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddTemplateAsync(template, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.AdmitCardManagement, "AdmitCardTemplateCreated", nameof(AdmitCardTemplate), template.Id.ToString(), "Success", "Admit card template created.", schoolId, currentUserContext.UserId, cancellationToken);
        return new AdmitCardTemplateDto
        {
            Id = template.Id,
            SchoolId = schoolId,
            TemplateName = template.TemplateName,
            SchoolDetails = template.SchoolDetails,
            LogoUrl = template.LogoUrl,
            Version = template.Version
        };
    }

    public async Task<IReadOnlyCollection<GeneratedAdmitCardDto>> GenerateAsync(GenerateAdmitCardsRequestDto request, CancellationToken cancellationToken)
    {
        await generateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var template = await repository.GetTemplateByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new NotFoundException("Admit card template not found.", "admit_card_template_not_found");
        if (template.SchoolId != schoolId)
        {
            throw new ForbiddenException("Admit card access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        var exam = await repository.GetExamByIdAsync(request.ExamId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.", "exam_not_found");
        if (exam.SchoolId != schoolId)
        {
            throw new ForbiddenException("Admit card access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        var requestedStudentIds = request.Students.Select(x => x.StudentId).Distinct().ToArray();
        var students = await repository.GetStudentsAsync(schoolId, requestedStudentIds, exam.ClassId, exam.SectionId, cancellationToken);
        if (students.Count != requestedStudentIds.Length)
        {
            throw new BadRequestException("One or more students are not eligible for the selected exam.", "invalid_admit_card_students");
        }

        var cards = new List<GeneratedAdmitCardDto>();
        foreach (var studentRequest in request.Students)
        {
            var student = students.First(x => x.Id == studentRequest.StudentId);
            var existingCard = await repository.GetGeneratedCardAsync(schoolId, exam.Id, student.Id, cancellationToken);
            var card = existingCard
                ?? new GeneratedAdmitCard
                {
                    SchoolId = schoolId,
                    TemplateId = template.Id,
                    ExamId = exam.Id,
                    StudentId = student.Id,
                    CreatedBy = currentUserContext.UserId?.ToString()
                };

            card.SeatNumber = studentRequest.SeatNumber.Trim();
            card.RoomNumber = studentRequest.RoomNumber?.Trim();
            card.Instructions = request.Instructions?.Trim();
            card.SnapshotJson = JsonSerializer.Serialize(new
            {
                school = template.SchoolDetails,
                exam = new
                {
                    exam.Id,
                    exam.Title,
                    exam.StartDate,
                    exam.EndDate
                },
                student = new
                {
                    student.Id,
                    name = $"{student.FirstName} {student.LastName}".Trim(),
                    student.RollNumber,
                    seatNumber = card.SeatNumber,
                    roomNumber = card.RoomNumber
                }
            });

            if (existingCard is null)
            {
                await repository.AddGeneratedCardAsync(card, cancellationToken);
            }

            cards.Add(new GeneratedAdmitCardDto
            {
                Id = card.Id,
                ExamId = exam.Id,
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                RollNumber = student.RollNumber,
                SeatNumber = card.SeatNumber,
                RoomNumber = card.RoomNumber,
                SnapshotJson = card.SnapshotJson
            });
        }

        await repository.SaveChangesAsync(cancellationToken);
        return cards;
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            var schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            return schoolId;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        return currentUserContext.SchoolId.Value;
    }
}
