using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Persistence;

public sealed class SchoolErpDbContext : DbContext, IUnitOfWork
{
    public SchoolErpDbContext(DbContextOptions<SchoolErpDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AcademicSession> AcademicSessions => Set<AcademicSession>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceSummary> AttendanceSummaries => Set<AttendanceSummary>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<FeeCategory> FeeCategories => Set<FeeCategory>();
    public DbSet<FeeInstallment> FeeInstallments => Set<FeeInstallment>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<FineRule> FineRules => Set<FineRule>();
    public DbSet<GuardianDetails> GuardianDetails => Set<GuardianDetails>();
    public DbSet<HomeworkAssignment> HomeworkAssignments => Set<HomeworkAssignment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PlanModuleEntitlement> PlanModuleEntitlements => Set<PlanModuleEntitlement>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();
    public DbSet<QuizSubmission> QuizSubmissions => Set<QuizSubmission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<SchoolSubscription> SchoolSubscriptions => Set<SchoolSubscription>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentAcademicInfo> StudentAcademicInfos => Set<StudentAcademicInfo>();
    public DbSet<StudentAttendanceSummary> StudentAttendanceSummaries => Set<StudentAttendanceSummary>();
    public DbSet<StudentDocument> StudentDocuments => Set<StudentDocument>();
    public DbSet<StudentFeeAssignment> StudentFeeAssignments => Set<StudentFeeAssignment>();
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Syllabus> Syllabi => Set<Syllabus>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeacherClassAssignment> TeacherClassAssignments => Set<TeacherClassAssignment>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<AcademicSession>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        modelBuilder.Entity<Class>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        modelBuilder.Entity<Campus>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        modelBuilder.Entity<Section>().HasIndex(x => new { x.ClassId, x.Name }).IsUnique();
        modelBuilder.Entity<Admission>().HasIndex(x => new { x.SchoolId, x.AdmissionNumber }).IsUnique();
        modelBuilder.Entity<AttendanceSession>().HasIndex(x => new { x.SchoolId, x.ClassId, x.SectionId, x.AttendanceDate }).IsUnique();
        modelBuilder.Entity<AttendanceRecord>().HasIndex(x => new { x.SchoolId, x.StudentId, x.AttendanceDate }).IsUnique();
        modelBuilder.Entity<AttendanceSummary>().HasIndex(x => new { x.SchoolId, x.StudentId, x.Year, x.Month }).IsUnique();
        modelBuilder.Entity<GuardianDetails>().HasIndex(x => x.AdmissionId).IsUnique();
        modelBuilder.Entity<FeeCategory>().HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();
        modelBuilder.Entity<FeeStructure>().HasIndex(x => new { x.SchoolId, x.FeeCategoryId, x.ClassId, x.SectionId, x.Name, x.EffectiveFromDate }).IsUnique();
        modelBuilder.Entity<StudentFeeAssignment>().HasIndex(x => new { x.SchoolId, x.StudentId, x.FeeStructureId, x.AcademicSessionId }).IsUnique();
        modelBuilder.Entity<FineRule>().HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();
        modelBuilder.Entity<Invoice>().HasIndex(x => new { x.SchoolId, x.InvoiceNumber }).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => new { x.SchoolId, x.ClassId, x.SectionId, x.AcademicSessionId, x.RollNumber }).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => x.AdmissionId).IsUnique().HasFilter("[AdmissionId] IS NOT NULL");
        modelBuilder.Entity<StudentAcademicInfo>().HasIndex(x => x.StudentId).IsUnique();
        modelBuilder.Entity<StudentAttendanceSummary>().HasIndex(x => x.StudentId).IsUnique();
        modelBuilder.Entity<Teacher>().HasIndex(x => new { x.SchoolId, x.EmployeeCode }).IsUnique();
        modelBuilder.Entity<Subject>().HasIndex(x => new { x.SchoolId, x.Code }).IsUnique();
        modelBuilder.Entity<Syllabus>().HasIndex(x => new { x.SchoolId, x.SubjectId, x.ClassId, x.AcademicSessionId }).IsUnique();
        modelBuilder.Entity<TeacherSubject>().HasIndex(x => new { x.TeacherId, x.SubjectId }).IsUnique();
        modelBuilder.Entity<TeacherClassAssignment>().HasIndex(x => new { x.TeacherId, x.ClassId, x.SectionId, x.AcademicSessionId }).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => new { x.TenantId, x.NormalizedEmail }).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        modelBuilder.Entity<Module>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Permission>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Quiz>().HasIndex(x => new { x.SchoolId, x.ClassId, x.SectionId, x.SubjectId, x.Title, x.StartDate }).IsUnique();
        modelBuilder.Entity<QuizSubmission>().HasIndex(x => new { x.SchoolId, x.QuizId, x.StudentId }).IsUnique();
        modelBuilder.Entity<QuizResult>().HasIndex(x => new { x.SchoolId, x.QuizId, x.StudentId }).IsUnique();
        modelBuilder.Entity<SubscriptionPlan>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<SubscriptionPlan>().Property(x => x.Price).HasPrecision(18, 2);
        modelBuilder.Entity<AttendanceSession>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AttendanceRecord>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<AttendanceSummary>().Property(x => x.AttendancePercentage).HasPrecision(5, 2);
        modelBuilder.Entity<AttendanceSummary>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<FeeCategory>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<FeeStructure>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<FeeStructure>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<StudentFeeAssignment>().Property(x => x.AssignedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<StudentFeeAssignment>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<FineRule>().Property(x => x.FlatAmount).HasPrecision(18, 2);
        modelBuilder.Entity<FineRule>().Property(x => x.DailyAmount).HasPrecision(18, 2);
        modelBuilder.Entity<FineRule>().Property(x => x.MaximumAmount).HasPrecision(18, 2);
        modelBuilder.Entity<FineRule>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Invoice>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.PaidAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.PendingAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.FineAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<FeeInstallment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Quiz>().Property(x => x.TotalMarks).HasPrecision(18, 2);
        modelBuilder.Entity<Quiz>().Property(x => x.PassingMarks).HasPrecision(18, 2);
        modelBuilder.Entity<Quiz>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<QuizQuestion>().Property(x => x.Marks).HasPrecision(18, 2);
        modelBuilder.Entity<QuizSubmission>().Property(x => x.AutoEvaluatedMarks).HasPrecision(18, 2);
        modelBuilder.Entity<QuizSubmission>().Property(x => x.ManualAdjustmentMarks).HasPrecision(18, 2);
        modelBuilder.Entity<QuizResult>().Property(x => x.ObtainedMarks).HasPrecision(18, 2);
        modelBuilder.Entity<QuizResult>().Property(x => x.TotalMarks).HasPrecision(18, 2);
        modelBuilder.Entity<QuizResult>().Property(x => x.Percentage).HasPrecision(5, 2);
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<UserPermission>().HasIndex(x => new { x.UserId, x.ModuleId }).IsUnique();

        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<PlanModuleEntitlement>().HasKey(x => new { x.SubscriptionPlanId, x.ModuleId });

        modelBuilder.Entity<School>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AcademicSession>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Admission>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AttendanceSession>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AttendanceRecord>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AttendanceSummary>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Campus>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Class>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FeeCategory>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FeeInstallment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FeeStructure>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FineRule>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<GuardianDetails>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<HomeworkAssignment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Module>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<PaymentTransaction>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<SubscriptionPlan>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Quiz>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<QuizOption>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<QuizQuestion>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<QuizResult>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<QuizSubmission>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<SchoolSubscription>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Section>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudentAcademicInfo>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudentAttendanceSummary>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudentDocument>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudentFeeAssignment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudyMaterial>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Subject>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Syllabus>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Teacher>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<TeacherClassAssignment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<TeacherSubject>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<UserPermission>().HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Subscriptions)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Campuses)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<School>()
            .HasMany(x => x.Users)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Permission>()
            .HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasMany(x => x.Permissions)
            .WithOne(x => x.Module)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubscriptionPlan>()
            .HasMany(x => x.PlanModuleEntitlements)
            .WithOne(x => x.SubscriptionPlan)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasMany(x => x.PlanModuleEntitlements)
            .WithOne(x => x.Module)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubscriptionPlan>()
            .HasMany(x => x.SchoolSubscriptions)
            .WithOne(x => x.SubscriptionPlan)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(x => x.UserPermissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasMany(x => x.UserPermissions)
            .WithOne(x => x.Module)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AcademicSession>()
            .HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Class>()
            .HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Section>()
            .HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Section>()
            .HasOne(x => x.Class)
            .WithMany(x => x.Sections)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admission>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceSession>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceSession>()
            .HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceSession>()
            .HasOne(x => x.Section)
            .WithMany()
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceSession>()
            .HasOne(x => x.MarkedByUser)
            .WithMany()
            .HasForeignKey(x => x.MarkedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.AttendanceSession)
            .WithMany(x => x.Records)
            .HasForeignKey(x => x.AttendanceSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.Section)
            .WithMany()
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.MarkedByUser)
            .WithMany()
            .HasForeignKey(x => x.MarkedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AttendanceSummary>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceSummary>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admission>()
            .HasOne(x => x.AppliedClass)
            .WithMany(x => x.Admissions)
            .HasForeignKey(x => x.AppliedClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admission>()
            .HasOne(x => x.AcademicSession)
            .WithMany(x => x.Admissions)
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GuardianDetails>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeCategory>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeStructure>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeStructure>()
            .HasOne(x => x.FeeCategory)
            .WithMany(x => x.FeeStructures)
            .HasForeignKey(x => x.FeeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeStructure>()
            .HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeStructure>()
            .HasOne(x => x.Section)
            .WithMany()
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentFeeAssignment>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentFeeAssignment>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentFeeAssignment>()
            .HasOne(x => x.FeeStructure)
            .WithMany(x => x.StudentFeeAssignments)
            .HasForeignKey(x => x.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentFeeAssignment>()
            .HasOne(x => x.AcademicSession)
            .WithMany()
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FineRule>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(x => x.StudentFeeAssignment)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.StudentFeeAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(x => x.FineRule)
            .WithMany()
            .HasForeignKey(x => x.FineRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(x => x.Invoice)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FeeInstallment>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeeInstallment>()
            .HasOne(x => x.Invoice)
            .WithMany(x => x.Installments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuardianDetails>()
            .HasOne(x => x.Admission)
            .WithOne(x => x.GuardianDetails)
            .HasForeignKey<GuardianDetails>(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.Admission)
            .WithOne(x => x.Student)
            .HasForeignKey<Student>(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.Class)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.Section)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.AcademicSession)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentAcademicInfo>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentAcademicInfo>()
            .HasOne(x => x.Student)
            .WithOne(x => x.AcademicInfo)
            .HasForeignKey<StudentAcademicInfo>(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentAttendanceSummary>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentAttendanceSummary>()
            .HasOne(x => x.Student)
            .WithOne(x => x.AttendanceSummary)
            .HasForeignKey<StudentAttendanceSummary>(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentDocument>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentDocument>()
            .HasOne(x => x.Student)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Teacher>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subject>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherSubject>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherSubject>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.Subjects)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherSubject>()
            .HasOne(x => x.Subject)
            .WithMany(x => x.TeacherSubjects)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherClassAssignment>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherClassAssignment>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.ClassAssignments)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherClassAssignment>()
            .HasOne(x => x.Class)
            .WithMany(x => x.TeacherClassAssignments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherClassAssignment>()
            .HasOne(x => x.Section)
            .WithMany(x => x.TeacherClassAssignments)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherClassAssignment>()
            .HasOne(x => x.AcademicSession)
            .WithMany(x => x.TeacherClassAssignments)
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Syllabus>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Syllabus>()
            .HasOne(x => x.Subject)
            .WithMany(x => x.Syllabi)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Syllabus>()
            .HasOne(x => x.Class)
            .WithMany(x => x.Syllabi)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Syllabus>()
            .HasOne(x => x.AcademicSession)
            .WithMany(x => x.Syllabi)
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudyMaterial>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudyMaterial>()
            .HasOne(x => x.Subject)
            .WithMany(x => x.StudyMaterials)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudyMaterial>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.StudyMaterials)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(x => x.Section)
            .WithMany()
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizQuestion>()
            .HasOne(x => x.Quiz)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizOption>()
            .HasOne(x => x.Question)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizSubmission>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizSubmission>()
            .HasOne(x => x.Quiz)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizSubmission>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizResult>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizResult>()
            .HasOne(x => x.Quiz)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizResult>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizResult>()
            .HasOne(x => x.QuizSubmission)
            .WithOne(x => x.Result)
            .HasForeignKey<QuizResult>(x => x.QuizSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HomeworkAssignment>()
            .HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HomeworkAssignment>()
            .HasOne(x => x.Subject)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HomeworkAssignment>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HomeworkAssignment>()
            .HasOne(x => x.Class)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HomeworkAssignment>()
            .HasOne(x => x.Section)
            .WithMany(x => x.HomeworkAssignments)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
