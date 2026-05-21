using FluentValidation;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Fees.Models;

public sealed class CreateFeeCategoryRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsRecurring { get; init; }
}

public sealed class CreateFeeStructureRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid FeeCategoryId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime EffectiveFromDate { get; init; }
    public DateTime? EffectiveToDate { get; init; }
}

public sealed class CreateFineRuleRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int GraceDays { get; init; }
    public decimal FlatAmount { get; init; }
    public decimal DailyAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
}

public sealed class AssignStudentFeesRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid FeeStructureId { get; init; }
    public Guid? AcademicSessionId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public IReadOnlyCollection<Guid> StudentIds { get; init; } = Array.Empty<Guid>();
}

public sealed class GenerateInvoiceInstallmentDto
{
    public string Title { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public decimal Amount { get; init; }
}

public sealed class GenerateInvoiceRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public Guid? StudentFeeAssignmentId { get; init; }
    public Guid? FineRuleId { get; init; }
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal TotalAmount { get; init; }
    public string? ReminderMetadata { get; init; }
    public IReadOnlyCollection<GenerateInvoiceInstallmentDto> Installments { get; init; } = Array.Empty<GenerateInvoiceInstallmentDto>();
}

public sealed class RecordPaymentRequestDto
{
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? TransactionReference { get; init; }
    public decimal Amount { get; init; }
    public string? Remarks { get; init; }
}

public sealed class FeeCategoryDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsRecurring { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class FeeStructureDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid FeeCategoryId { get; init; }
    public string FeeCategoryName { get; init; } = string.Empty;
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime EffectiveFromDate { get; init; }
    public DateTime? EffectiveToDate { get; init; }
    public bool IsActive { get; init; }
}

public sealed class FineRuleDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int GraceDays { get; init; }
    public decimal FlatAmount { get; init; }
    public decimal DailyAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
    public bool IsActive { get; init; }
}

public sealed class StudentFeeAssignmentDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public Guid FeeStructureId { get; init; }
    public string FeeStructureName { get; init; } = string.Empty;
    public decimal AssignedAmount { get; init; }
    public DateTime AssignedDate { get; init; }
    public bool IsActive { get; init; }
}

public sealed class PaymentTransactionDto
{
    public Guid Id { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? TransactionReference { get; init; }
    public decimal Amount { get; init; }
    public string? Remarks { get; init; }
}

public sealed class FeeInstallmentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public decimal Amount { get; init; }
    public bool IsPaid { get; init; }
    public DateTime? PaidDate { get; init; }
}

public sealed class InvoiceDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal FineAmount { get; init; }
    public InvoiceStatus Status { get; init; }
    public string? ReminderMetadata { get; init; }
    public IReadOnlyCollection<FeeInstallmentDto> Installments { get; init; } = Array.Empty<FeeInstallmentDto>();
    public IReadOnlyCollection<PaymentTransactionDto> Payments { get; init; } = Array.Empty<PaymentTransactionDto>();
}

public sealed class FeeInvoiceListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? StudentId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class FeeAnalyticsDto
{
    public decimal PendingFees { get; init; }
    public decimal CollectedAmount { get; init; }
    public decimal OverdueAmount { get; init; }
    public int OverdueInvoices { get; init; }
    public IReadOnlyCollection<RevenueTrendPointDto> MonthlyRevenue { get; init; } = Array.Empty<RevenueTrendPointDto>();
}

public sealed class RevenueTrendPointDto
{
    public string Label { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public sealed class FeeExportRowDto
{
    public string InvoiceNumber { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class CreateFeeCategoryRequestDtoValidator : AbstractValidator<CreateFeeCategoryRequestDto>
{
    public CreateFeeCategoryRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateFeeStructureRequestDtoValidator : AbstractValidator<CreateFeeStructureRequestDto>
{
    public CreateFeeStructureRequestDtoValidator()
    {
        RuleFor(x => x.FeeCategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public sealed class CreateFineRuleRequestDtoValidator : AbstractValidator<CreateFineRuleRequestDto>
{
    public CreateFineRuleRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.GraceDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FlatAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DailyAmount).GreaterThanOrEqualTo(0);
    }
}

public sealed class AssignStudentFeesRequestDtoValidator : AbstractValidator<AssignStudentFeesRequestDto>
{
    public AssignStudentFeesRequestDtoValidator()
    {
        RuleFor(x => x.FeeStructureId).NotEmpty();
        RuleFor(x => x).Must(x => x.StudentIds.Count > 0 || x.ClassId.HasValue).WithMessage("Either StudentIds or ClassId must be provided.");
    }
}

public sealed class GenerateInvoiceRequestDtoValidator : AbstractValidator<GenerateInvoiceRequestDto>
{
    public GenerateInvoiceRequestDtoValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate);
        RuleForEach(x => x.Installments).ChildRules(installment =>
        {
            installment.RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
            installment.RuleFor(x => x.Amount).GreaterThan(0);
        });
    }
}

public sealed class RecordPaymentRequestDtoValidator : AbstractValidator<RecordPaymentRequestDto>
{
    public RecordPaymentRequestDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.TransactionReference).MaximumLength(120);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public sealed class FeeInvoiceListRequestDtoValidator : SearchablePagedRequestValidator<FeeInvoiceListRequestDto>
{
}
