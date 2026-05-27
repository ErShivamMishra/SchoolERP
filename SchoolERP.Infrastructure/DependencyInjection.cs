using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.Admissions.Interfaces;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Students.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Application.Features.Teachers.Interfaces;
using SchoolERP.Application.Features.Study.Interfaces;
using SchoolERP.Application.Features.Attendance.Interfaces;
using SchoolERP.Application.Features.Fees.Interfaces;
using SchoolERP.Application.Features.Quizzes.Interfaces;
using SchoolERP.Application.Features.Dashboard.Interfaces;
using SchoolERP.Application.Features.AuditLogs.Interfaces;
using SchoolERP.Application.Features.Reports.Interfaces;
using SchoolERP.Application.Features.Results.Interfaces;
using SchoolERP.Application.Features.Notices.Interfaces;
using SchoolERP.Application.Features.Communications.Interfaces;
using SchoolERP.Application.Features.Transport.Interfaces;
using SchoolERP.Application.Features.Gallery.Interfaces;
using SchoolERP.Infrastructure.Options;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Services;
using System.Text.Json;

namespace SchoolERP.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultLocalDbConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SchoolERPDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            connectionString = TryReadConnectionStringFromFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));

            if (string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(environmentName))
            {
                connectionString = TryReadConnectionStringFromFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{environmentName}.json"));
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultLocalDbConnectionString;
        }

        services.AddDbContext<SchoolErpDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthPolicyProvider, AuthPolicyProvider>();
        services.AddScoped<IAccessControlService, AccessControlService>();
        services.AddScoped<IAdmissionRepository, AdmissionRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddScoped<ICommunicationRepository, CommunicationRepository>();
        services.AddScoped<ITransportRepository, TransportRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IPermissionManagementRepository, PermissionManagementRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    private static string? TryReadConnectionStringFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
        {
            return null;
        }

        if (!connectionStrings.TryGetProperty("DefaultConnection", out var connectionString))
        {
            return null;
        }

        return connectionString.GetString();
    }
}
