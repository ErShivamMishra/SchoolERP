using System.Text.Json;
using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.IdCards.Interfaces;
using SchoolERP.Application.Features.IdCards.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.IdCards.Services;

public sealed class IdCardService(
    IIdCardRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateIdCardTemplateRequestDto> createValidator,
    IValidator<GenerateStudentIdCardsRequestDto> studentValidator,
    IValidator<GenerateTeacherIdCardsRequestDto> teacherValidator) : IIdCardService
{
    public async Task<IdCardTemplateDto> CreateTemplateAsync(CreateIdCardTemplateRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var version = await repository.GetNextTemplateVersionAsync(schoolId, request.TemplateName.Trim(), cancellationToken);

        var template = new IdCardTemplate
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
        await auditService.WriteAsync(ModuleCodes.IdCardManagement, "IdCardTemplateCreated", nameof(IdCardTemplate), template.Id.ToString(), "Success", "ID card template created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapTemplate(template);
    }

    public async Task<IReadOnlyCollection<GeneratedIdCardDto>> GenerateStudentCardsAsync(GenerateStudentIdCardsRequestDto request, CancellationToken cancellationToken)
    {
        await studentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var template = await GetManagedTemplateAsync(request.TemplateId, schoolId, cancellationToken);
        var students = await repository.GetStudentsAsync(schoolId, request.StudentIds.Distinct().ToArray(), cancellationToken);
        if (students.Count != request.StudentIds.Distinct().Count())
        {
            throw new BadRequestException("One or more students are invalid for the current school.", "invalid_id_card_students");
        }

        var cards = new List<GeneratedIdCardDto>();
        foreach (var student in students)
        {
            var existingCard = await repository.GetGeneratedStudentCardAsync(schoolId, template.Id, student.Id, cancellationToken);
            var card = existingCard
                ?? new GeneratedIdCard
                {
                    SchoolId = schoolId,
                    TemplateId = template.Id,
                    StudentId = student.Id,
                    CardHolderType = "Student",
                    CreatedBy = currentUserContext.UserId?.ToString()
                };

            card.CardHolderName = $"{student.FirstName} {student.LastName}".Trim();
            card.CardIdentifier = student.RollNumber;
            card.QrCodePayload = request.IncludeQrMetadata ? $"student:{student.Id}" : null;
            card.BarcodePayload = student.RollNumber;
            card.SnapshotJson = JsonSerializer.Serialize(new
            {
                school = template.SchoolDetails,
                student = new
                {
                    student.Id,
                    name = card.CardHolderName,
                    student.RollNumber,
                    className = student.Class?.Name,
                    sectionName = student.Section?.Name
                }
            });

            if (existingCard is null)
            {
                await repository.AddGeneratedCardAsync(card, cancellationToken);
            }

            cards.Add(MapCard(card));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return cards;
    }

    public async Task<IReadOnlyCollection<GeneratedIdCardDto>> GenerateTeacherCardsAsync(GenerateTeacherIdCardsRequestDto request, CancellationToken cancellationToken)
    {
        await teacherValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var template = await GetManagedTemplateAsync(request.TemplateId, schoolId, cancellationToken);
        var teachers = await repository.GetTeachersAsync(schoolId, request.TeacherIds.Distinct().ToArray(), cancellationToken);
        if (teachers.Count != request.TeacherIds.Distinct().Count())
        {
            throw new BadRequestException("One or more teachers are invalid for the current school.", "invalid_id_card_teachers");
        }

        var cards = new List<GeneratedIdCardDto>();
        foreach (var teacher in teachers)
        {
            var existingCard = await repository.GetGeneratedTeacherCardAsync(schoolId, template.Id, teacher.Id, cancellationToken);
            var card = existingCard
                ?? new GeneratedIdCard
                {
                    SchoolId = schoolId,
                    TemplateId = template.Id,
                    TeacherId = teacher.Id,
                    CardHolderType = "Teacher",
                    CreatedBy = currentUserContext.UserId?.ToString()
                };

            card.CardHolderName = $"{teacher.FirstName} {teacher.LastName}".Trim();
            card.CardIdentifier = teacher.EmployeeCode;
            card.QrCodePayload = request.IncludeQrMetadata ? $"teacher:{teacher.Id}" : null;
            card.BarcodePayload = teacher.EmployeeCode;
            card.SnapshotJson = JsonSerializer.Serialize(new
            {
                school = template.SchoolDetails,
                teacher = new
                {
                    teacher.Id,
                    name = card.CardHolderName,
                    teacher.EmployeeCode,
                    teacher.MobileNumber
                }
            });

            if (existingCard is null)
            {
                await repository.AddGeneratedCardAsync(card, cancellationToken);
            }

            cards.Add(MapCard(card));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return cards;
    }

    private async Task<IdCardTemplate> GetManagedTemplateAsync(Guid templateId, Guid schoolId, CancellationToken cancellationToken)
    {
        var template = await repository.GetTemplateByIdAsync(templateId, cancellationToken)
            ?? throw new NotFoundException("ID card template not found.", "id_card_template_not_found");
        if (template.SchoolId != schoolId)
        {
            throw new ForbiddenException("ID card access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return template;
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

    private static IdCardTemplateDto MapTemplate(IdCardTemplate template) => new()
    {
        Id = template.Id,
        SchoolId = template.SchoolId,
        TemplateName = template.TemplateName,
        LogoUrl = template.LogoUrl,
        SchoolDetails = template.SchoolDetails,
        LayoutJson = template.LayoutJson,
        Version = template.Version,
        IsActive = template.IsActive
    };

    private static GeneratedIdCardDto MapCard(GeneratedIdCard card) => new()
    {
        Id = card.Id,
        TemplateId = card.TemplateId,
        StudentId = card.StudentId,
        TeacherId = card.TeacherId,
        CardHolderType = card.CardHolderType,
        CardHolderName = card.CardHolderName,
        CardIdentifier = card.CardIdentifier,
        QrCodePayload = card.QrCodePayload,
        BarcodePayload = card.BarcodePayload,
        SnapshotJson = card.SnapshotJson
    };
}
