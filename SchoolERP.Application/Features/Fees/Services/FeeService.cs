using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Interfaces;
using SchoolERP.Application.Features.Fees.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using System.Text.Json;

namespace SchoolERP.Application.Features.Fees.Services;

public sealed class FeeService(
    IFeeRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateFeeCategoryRequestDto> categoryValidator,
    IValidator<CreateFeeStructureRequestDto> structureValidator,
    IValidator<CreateFineRuleRequestDto> fineRuleValidator,
    IValidator<AssignStudentFeesRequestDto> assignmentValidator,
    IValidator<GenerateInvoiceRequestDto> invoiceValidator,
    IValidator<RecordPaymentRequestDto> paymentValidator,
    IValidator<FeeInvoiceListRequestDto> invoiceListValidator) : IFeeService
{
    public async Task<FeeCategoryDto> CreateCategoryAsync(CreateFeeCategoryRequestDto request, CancellationToken cancellationToken)
    {
        await categoryValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var category = new FeeCategory
        {
            SchoolId = schoolId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsRecurring = request.IsRecurring,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddFeeCategoryAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "FeeCategoryCreated", nameof(FeeCategory), category.Id.ToString(), "Success", "Fee category created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapCategory(category);
    }

    public async Task<FeeStructureDto> CreateStructureAsync(CreateFeeStructureRequestDto request, CancellationToken cancellationToken)
    {
        await structureValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var category = await repository.GetFeeCategoryByIdAsync(request.FeeCategoryId, cancellationToken)
            ?? throw new NotFoundException("Fee category not found.", "fee_category_not_found");
        EnsureSchoolAccess(category.SchoolId);

        var structure = new FeeStructure
        {
            SchoolId = schoolId,
            FeeCategoryId = request.FeeCategoryId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            EffectiveFromDate = request.EffectiveFromDate.Date,
            EffectiveToDate = request.EffectiveToDate?.Date,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddFeeStructureAsync(structure, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        structure.FeeCategory = category;
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "FeeStructureCreated", nameof(FeeStructure), structure.Id.ToString(), "Success", "Fee structure created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapStructure(structure);
    }

    public async Task<FineRuleDto> CreateFineRuleAsync(CreateFineRuleRequestDto request, CancellationToken cancellationToken)
    {
        await fineRuleValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var rule = new FineRule
        {
            SchoolId = schoolId,
            Name = request.Name.Trim(),
            GraceDays = request.GraceDays,
            FlatAmount = request.FlatAmount,
            DailyAmount = request.DailyAmount,
            MaximumAmount = request.MaximumAmount,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddFineRuleAsync(rule, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "FineRuleCreated", nameof(FineRule), rule.Id.ToString(), "Success", "Fine rule created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapFineRule(rule);
    }

    public async Task<IReadOnlyCollection<StudentFeeAssignmentDto>> AssignFeesAsync(AssignStudentFeesRequestDto request, CancellationToken cancellationToken)
    {
        await assignmentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var structure = await repository.GetFeeStructureByIdAsync(request.FeeStructureId, cancellationToken)
            ?? throw new NotFoundException("Fee structure not found.", "fee_structure_not_found");
        EnsureSchoolAccess(structure.SchoolId);

        var students = await repository.GetStudentsForAssignmentAsync(schoolId, request.ClassId, request.SectionId, request.StudentIds, cancellationToken);
        if (students.Count == 0)
        {
            throw new BadRequestException("No eligible students found for fee assignment.", "no_students_for_fee_assignment");
        }

        var assignments = students.Select(x => new StudentFeeAssignment
        {
            SchoolId = schoolId,
            StudentId = x.Id,
            FeeStructureId = structure.Id,
            AcademicSessionId = request.AcademicSessionId,
            AssignedAmount = structure.Amount,
            AssignedDate = DateTime.UtcNow.Date,
            CreatedBy = currentUserContext.UserId?.ToString()
        }).ToArray();

        await repository.AddStudentFeeAssignmentsAsync(assignments, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        structure.StudentFeeAssignments = assignments;
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "StudentFeesAssigned", nameof(StudentFeeAssignment), null, "Success", $"Fees assigned to {assignments.Length} students.", schoolId, currentUserContext.UserId, null, JsonSerializer.Serialize(new { Count = assignments.Length, Structure = structure.Name }), null, null, cancellationToken);
        return assignments.Select(x => MapAssignment(x, structure, students.First(y => y.Id == x.StudentId))).ToArray();
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequestDto request, CancellationToken cancellationToken)
    {
        await invoiceValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var student = await repository.GetStudentByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student not found.", "student_not_found");
        EnsureSchoolAccess(student.SchoolId);

        FineRule? fineRule = null;
        if (request.FineRuleId.HasValue)
        {
            fineRule = await repository.GetFineRuleByIdAsync(request.FineRuleId.Value, cancellationToken)
                ?? throw new NotFoundException("Fine rule not found.", "fine_rule_not_found");
            EnsureSchoolAccess(fineRule.SchoolId);
        }

        var invoice = new Invoice
        {
            SchoolId = schoolId,
            StudentId = request.StudentId,
            StudentFeeAssignmentId = request.StudentFeeAssignmentId,
            FineRuleId = request.FineRuleId,
            InvoiceNumber = await repository.GenerateInvoiceNumberAsync(schoolId, cancellationToken),
            InvoiceDate = request.InvoiceDate.Date,
            DueDate = request.DueDate.Date,
            TotalAmount = request.TotalAmount,
            PendingAmount = request.TotalAmount,
            PaidAmount = 0,
            FineAmount = CalculateFine(fineRule, request.DueDate.Date, DateTime.UtcNow.Date),
            ReminderMetadata = request.ReminderMetadata?.Trim(),
            Status = request.DueDate.Date < DateTime.UtcNow.Date ? InvoiceStatus.Overdue : InvoiceStatus.Pending,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        invoice.Installments = request.Installments.Select(x => new FeeInstallment
        {
            SchoolId = schoolId,
            InvoiceId = invoice.Id,
            Title = x.Title.Trim(),
            DueDate = x.DueDate.Date,
            Amount = x.Amount,
            CreatedBy = currentUserContext.UserId?.ToString()
        }).ToList();

        await repository.AddInvoiceAsync(invoice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        invoice.Student = student;
        invoice.FineRule = fineRule;
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "InvoiceGenerated", nameof(Invoice), invoice.Id.ToString(), "Success", $"Invoice {invoice.InvoiceNumber} generated.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapInvoice(invoice);
    }

    public async Task<InvoiceDto> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequestDto request, CancellationToken cancellationToken)
    {
        await paymentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var invoice = await repository.GetInvoiceByIdAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException("Invoice not found.", "invoice_not_found");
        EnsureSchoolAccess(invoice.SchoolId);

        if (request.Amount > invoice.PendingAmount)
        {
            throw new ConflictException("Payment amount cannot exceed the pending balance.", "invoice_overpayment");
        }

        var payment = new PaymentTransaction
        {
            SchoolId = invoice.SchoolId,
            InvoiceId = invoice.Id,
            PaymentDate = request.PaymentDate,
            PaymentMethod = request.PaymentMethod,
            TransactionReference = request.TransactionReference?.Trim(),
            Amount = request.Amount,
            Remarks = request.Remarks?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        invoice.PaidAmount += request.Amount;
        invoice.PendingAmount = invoice.TotalAmount + invoice.FineAmount - invoice.PaidAmount;
        invoice.Status = invoice.PendingAmount <= 0 ? InvoiceStatus.Paid : invoice.PaidAmount > 0 ? InvoiceStatus.Partial : invoice.Status;
        invoice.ModifiedAtUtc = DateTime.UtcNow;
        invoice.ModifiedBy = currentUserContext.UserId?.ToString();

        foreach (var installment in invoice.Installments.Where(x => !x.IsPaid).OrderBy(x => x.DueDate))
        {
            if (request.Amount >= installment.Amount && !installment.IsPaid)
            {
                installment.IsPaid = true;
                installment.PaidDate = request.PaymentDate;
            }
        }

        await repository.AddPaymentTransactionAsync(payment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        invoice.PaymentTransactions.Add(payment);
        await auditService.WriteAsync(ModuleCodes.FeeManagement, "InvoicePaymentRecorded", nameof(PaymentTransaction), payment.Id.ToString(), "Success", $"Payment recorded for invoice {invoice.InvoiceNumber}.", invoice.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapInvoice(invoice);
    }

    public async Task<PagedResult<InvoiceDto>> GetInvoicesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
    {
        await invoiceListValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetInvoicesPageAsync(schoolId, request.StudentId, request.Status, request.FromDate, request.ToDate, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<InvoiceDto>
        {
            Items = items.Select(MapInvoice).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyCollection<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid studentId, Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var payments = await repository.GetStudentPaymentsAsync(resolvedSchoolId, studentId, cancellationToken);
        return payments.Select(MapPayment).ToArray();
    }

    public async Task<FeeAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var analytics = await repository.GetAnalyticsAsync(resolvedSchoolId, cancellationToken);
        return new FeeAnalyticsDto
        {
            PendingFees = analytics.PendingFees,
            CollectedAmount = analytics.CollectedAmount,
            OverdueAmount = analytics.OverdueAmount,
            OverdueInvoices = analytics.OverdueInvoices,
            MonthlyRevenue = analytics.RevenueTrend.Select(x => new RevenueTrendPointDto
            {
                Label = $"{x.Year:D4}-{x.Month:D2}",
                Amount = x.Amount
            }).ToArray()
        };
    }

    public async Task<PagedResult<FeeExportRowDto>> ExportInvoicesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
    {
        var invoices = await GetInvoicesAsync(request, cancellationToken);
        return new PagedResult<FeeExportRowDto>
        {
            Items = invoices.Items.Select(x => new FeeExportRowDto
            {
                InvoiceNumber = x.InvoiceNumber,
                StudentName = x.StudentName,
                RollNumber = x.RollNumber,
                InvoiceDate = x.InvoiceDate,
                DueDate = x.DueDate,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.PaidAmount,
                PendingAmount = x.PendingAmount,
                Status = x.Status.ToString()
            }).ToArray(),
            PageNumber = invoices.PageNumber,
            PageSize = invoices.PageSize,
            TotalCount = invoices.TotalCount
        };
    }

    private decimal CalculateFine(FineRule? fineRule, DateTime dueDate, DateTime today)
    {
        if (fineRule is null || today <= dueDate.AddDays(fineRule.GraceDays))
        {
            return 0;
        }

        var overdueDays = (today - dueDate.AddDays(fineRule.GraceDays)).Days;
        var fine = fineRule.FlatAmount + (fineRule.DailyAmount * overdueDays);
        return fineRule.MaximumAmount.HasValue ? Math.Min(fine, fineRule.MaximumAmount.Value) : fine;
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

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Fee access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Fee access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Fee access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static FeeCategoryDto MapCategory(FeeCategory category) => new()
    {
        Id = category.Id,
        SchoolId = category.SchoolId,
        Name = category.Name,
        Description = category.Description,
        IsRecurring = category.IsRecurring,
        CreatedAt = category.CreatedAtUtc
    };

    private static FeeStructureDto MapStructure(FeeStructure structure) => new()
    {
        Id = structure.Id,
        SchoolId = structure.SchoolId,
        FeeCategoryId = structure.FeeCategoryId,
        FeeCategoryName = structure.FeeCategory?.Name ?? string.Empty,
        ClassId = structure.ClassId,
        SectionId = structure.SectionId,
        Name = structure.Name,
        Amount = structure.Amount,
        EffectiveFromDate = structure.EffectiveFromDate,
        EffectiveToDate = structure.EffectiveToDate,
        IsActive = structure.IsActive
    };

    private static FineRuleDto MapFineRule(FineRule rule) => new()
    {
        Id = rule.Id,
        SchoolId = rule.SchoolId,
        Name = rule.Name,
        GraceDays = rule.GraceDays,
        FlatAmount = rule.FlatAmount,
        DailyAmount = rule.DailyAmount,
        MaximumAmount = rule.MaximumAmount,
        IsActive = rule.IsActive
    };

    private static StudentFeeAssignmentDto MapAssignment(StudentFeeAssignment assignment, FeeStructure structure, Student student) => new()
    {
        Id = assignment.Id,
        StudentId = assignment.StudentId,
        StudentName = $"{student.FirstName} {student.LastName}".Trim(),
        RollNumber = student.RollNumber,
        FeeStructureId = assignment.FeeStructureId,
        FeeStructureName = structure.Name,
        AssignedAmount = assignment.AssignedAmount,
        AssignedDate = assignment.AssignedDate,
        IsActive = assignment.IsActive
    };

    private static PaymentTransactionDto MapPayment(PaymentTransaction payment) => new()
    {
        Id = payment.Id,
        PaymentDate = payment.PaymentDate,
        PaymentMethod = payment.PaymentMethod,
        TransactionReference = payment.TransactionReference,
        Amount = payment.Amount,
        Remarks = payment.Remarks
    };

    private static InvoiceDto MapInvoice(Invoice invoice) => new()
    {
        Id = invoice.Id,
        SchoolId = invoice.SchoolId,
        StudentId = invoice.StudentId,
        StudentName = $"{invoice.Student?.FirstName} {invoice.Student?.LastName}".Trim(),
        RollNumber = invoice.Student?.RollNumber ?? string.Empty,
        InvoiceNumber = invoice.InvoiceNumber,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        TotalAmount = invoice.TotalAmount,
        PaidAmount = invoice.PaidAmount,
        PendingAmount = invoice.PendingAmount,
        FineAmount = invoice.FineAmount,
        Status = invoice.Status,
        ReminderMetadata = invoice.ReminderMetadata,
        Installments = invoice.Installments.Select(x => new FeeInstallmentDto
        {
            Id = x.Id,
            Title = x.Title,
            DueDate = x.DueDate,
            Amount = x.Amount,
            IsPaid = x.IsPaid,
            PaidDate = x.PaidDate
        }).ToArray(),
        Payments = invoice.PaymentTransactions.Select(MapPayment).ToArray()
    };
}
