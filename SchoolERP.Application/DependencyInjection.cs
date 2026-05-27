using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Services;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.AccessControl.Services;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Modules.Services;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Schools.Services;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Staff.Services;
using SchoolERP.Application.Features.Admissions.Interfaces;
using SchoolERP.Application.Features.Admissions.Services;
using SchoolERP.Application.Features.Students.Interfaces;
using SchoolERP.Application.Features.Students.Services;
using SchoolERP.Application.Features.Attendance.Interfaces;
using SchoolERP.Application.Features.Attendance.Services;
using SchoolERP.Application.Features.Fees.Interfaces;
using SchoolERP.Application.Features.Fees.Services;
using SchoolERP.Application.Features.Quizzes.Interfaces;
using SchoolERP.Application.Features.Quizzes.Services;
using SchoolERP.Application.Features.Dashboard.Interfaces;
using SchoolERP.Application.Features.Dashboard.Services;
using SchoolERP.Application.Features.AuditLogs.Interfaces;
using SchoolERP.Application.Features.AuditLogs.Services;
using SchoolERP.Application.Features.Reports.Interfaces;
using SchoolERP.Application.Features.Reports.Services;
using SchoolERP.Application.Features.Results.Interfaces;
using SchoolERP.Application.Features.Results.Services;
using SchoolERP.Application.Features.Notices.Interfaces;
using SchoolERP.Application.Features.Notices.Services;
using SchoolERP.Application.Features.Communications.Interfaces;
using SchoolERP.Application.Features.Communications.Services;
using SchoolERP.Application.Features.Transport.Interfaces;
using SchoolERP.Application.Features.Transport.Services;
using SchoolERP.Application.Features.Gallery.Interfaces;
using SchoolERP.Application.Features.Gallery.Services;
using SchoolERP.Application.Features.IdCards.Interfaces;
using SchoolERP.Application.Features.IdCards.Services;
using SchoolERP.Application.Features.AdmitCards.Interfaces;
using SchoolERP.Application.Features.AdmitCards.Services;
using SchoolERP.Application.Features.ParentPortal.Interfaces;
using SchoolERP.Application.Features.ParentPortal.Services;
using SchoolERP.Application.Features.Teachers.Interfaces;
using SchoolERP.Application.Features.Teachers.Services;
using SchoolERP.Application.Features.Study.Interfaces;
using SchoolERP.Application.Features.Study.Services;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Services;

namespace SchoolERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IAdmissionService, AdmissionService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IFeeService, FeeService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<INoticeService, NoticeService>();
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<ITransportService, TransportService>();
        services.AddScoped<IGalleryService, GalleryService>();
        services.AddScoped<IIdCardService, IdCardService>();
        services.AddScoped<IAdmitCardService, AdmitCardService>();
        services.AddScoped<IParentPortalService, ParentPortalService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IStudyService, StudyService>();
        services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

        return services;
    }
}
