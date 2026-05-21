namespace SchoolERP.Domain.Constants;

public static class ModuleCodes
{
    public const string AdmissionManagement = "AdmissionManagement";
    public const string SchoolManagement = "SchoolManagement";
    public const string StaffManagement = "StaffManagement";
    public const string StudentManagement = "StudentManagement";
    public const string TeacherManagement = "TeacherManagement";
    public const string AttendanceManagement = "AttendanceManagement";
    public const string FeeManagement = "FeeManagement";
    public const string ResultManagement = "ResultManagement";
    public const string QuizManagement = "QuizManagement";
    public const string StudyManagement = "StudyManagement";
    public const string DashboardManagement = "DashboardManagement";
    public const string IdCardManagement = "IdCardManagement";
    public const string AdmitCardManagement = "AdmitCardManagement";

    public static readonly IReadOnlyCollection<string> Seeded = new[]
    {
        AdmissionManagement,
        SchoolManagement,
        StaffManagement,
        StudentManagement,
        TeacherManagement,
        AttendanceManagement,
        FeeManagement,
        ResultManagement,
        QuizManagement,
        StudyManagement,
        DashboardManagement,
        IdCardManagement,
        AdmitCardManagement
    };
}
