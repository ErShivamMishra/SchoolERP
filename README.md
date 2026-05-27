# SchoolERP Backend

Multi-tenant SaaS School ERP backend built with ASP.NET Core 8 Web API, EF Core, SQL Server, JWT authentication, and Clean Architecture.

## Solution Structure

- `SchoolERP.API`: controllers, middleware, Swagger, response wrappers, current-user context
- `SchoolERP.Application`: DTOs, validators, interfaces, business services
- `SchoolERP.Domain`: entities, enums, constants
- `SchoolERP.Infrastructure`: EF Core persistence, repositories, token/password/audit/access-control services, seeding, migrations

## Clean Architecture Rules

- Controllers use DTOs only
- Business rules live in application services
- Data access lives behind repositories and infrastructure services
- Domain entities are never returned directly from API responses
- API responses use the shared wrapper:

```json
{
  "success": true,
  "message": "Human readable message",
  "data": {},
  "errors": []
}
```

## Setup Flow

1. Update `SchoolERP.API/appsettings.json` if you want a non-default SQL Server connection.
2. Restore and build:

```powershell
dotnet restore
dotnet build SchoolERP.slnx
```

3. Apply migrations:

```powershell
dotnet ef database update --project SchoolERP.Infrastructure --startup-project SchoolERP.API
```

4. Run the API:

```powershell
dotnet run --project SchoolERP.API
```

5. Open Swagger in development:

- `http://localhost:5049/swagger`
- `https://localhost:7072/swagger`

Important:

- The app now uses EF Core migrations on startup.
- A legacy bootstrap step baselines old databases that were created with `EnsureCreated`, so existing local environments can still move forward safely.

## Authentication Flow

- `POST /api/v1/auth/login` returns `accessToken`, `refreshToken`, token expirations, and user metadata.
- Send protected requests with `Authorization: Bearer <accessToken>`.
- `POST /api/v1/auth/refresh-token` rotates refresh tokens.
- `POST /api/v1/auth/change-password` is required for temporary-password users before they can access normal protected APIs.
- JWT includes:
  - `UserId`
  - `SchoolId` / `tenant_id`
  - role claims
  - first-login / password-reset flags

Default seeded SuperAdmin:

- Email: `superadmin@schoolerp.local`
- Password: `SuperAdmin@123`

## SaaS / Tenant Structure

- `SuperAdmin` is platform-level and bypasses tenant module licensing and permission checks.
- `SchoolAdmin` is tenant-level and can manage only the current school.
- `Staff` is tenant-level and is permission-driven.
- Every tenant user is isolated by `SchoolId`.
- Subscription validation blocks tenant access when:
  - school is inactive
  - school is suspended
  - subscription is expired

## Module, Role, and Permission Model

Seeded modules:

- `AdmissionManagement`
- `SchoolManagement`
- `StaffManagement`
- `StudentManagement`
- `TeacherManagement`
- `AttendanceManagement`
- `FeeManagement`
- `ResultManagement`
- `QuizManagement`
- `StudyManagement`
- `DashboardManagement`
- `NoticeBoardManagement`
- `CommunicationManagement`
- `TransportManagement`
- `GalleryManagement`
- `IdCardManagement`
- `AdmitCardManagement`
- `ParentPortalManagement`

Permission model:

- Each module gets CRUD permission codes:
  - `{ModuleCode}.View`
  - `{ModuleCode}.Create`
  - `{ModuleCode}.Edit`
  - `{ModuleCode}.Delete`
- Role defaults are stored through `RolePermission`
- User-specific overrides are stored through `UserPermission`
- Effective access requires:
  - valid tenant context
  - active subscription
  - licensed module in the assigned plan
  - matching role permission or user permission

## Plan-Based Access

- `SubscriptionPlan` stores the SaaS plan
- `PlanModuleEntitlement` stores which modules are licensed for a plan
- `SchoolSubscription` stores the active plan assigned to a school
- Even if a user has CRUD permission, access is denied when the module is not licensed for the school plan

Current seed behavior:

- `BASIC` licenses `AdmissionManagement`, `StaffManagement`, `StudentManagement`, `TeacherManagement`, `AttendanceManagement`, `StudyManagement`, `DashboardManagement`, `ResultManagement`, `NoticeBoardManagement`
- `PREMIUM` licenses all seeded modules

## Existing and Implemented Modules

Day 1 APIs:

- System health
- Authentication
- School onboarding and school lifecycle management

Day 2 APIs:

- Staff Management
- User permission assignment
- Role permission lookup
- Module Management
- Subscription plan creation and assignment

Day 3 APIs:

- Admission Management
- Academic Session / Class / Section setup
- Student Management
- Teacher Management
- Study Management
- Tenant-organized file uploads for student documents, study materials, and homework attachments

Day 4 APIs:

- Attendance Management
- Fee Management
- Quiz and Online Examination Management
- Dashboard and analytics APIs
- Audit log query APIs
- Export and reporting APIs
- Direct exam and result management APIs
- Notice board and announcement APIs
- Parent-teacher communication APIs
- Transport management APIs
- Photo and video gallery APIs

## Module Organization

- `Features/Authentication`
- `Features/Schools`
- `Features/Staff`
- `Features/Modules`
- `Features/AccessControl`
- `Features/Subscriptions`
- `Features/Admissions`
- `Features/Students`
- `Features/Teachers`
- `Features/Study`
- `Features/Attendance`
- `Features/Fees`
- `Features/Quizzes`
- `Features/Dashboard`
- `Features/AuditLogs`
- `Features/Reports`
- `Features/Results`
- `Features/Notices`
- `Features/Communications`
- `Features/Transport`
- `Features/Gallery`
- `Features/IdCards`
- `Features/AdmitCards`
- `Features/ParentPortal`

Each feature contains:

- `Models`: request/response DTOs and validators
- `Interfaces`: service/repository contracts
- `Services`: business logic

## Middleware and Cross-Cutting Services

- `GlobalExceptionMiddleware`: standardized API errors
- `SubscriptionValidationMiddleware`: tenant school-state validation plus module/permission enforcement
- `ForcePasswordResetMiddleware`: blocks temporary-password users from business endpoints until password change is completed
- `CurrentUserContext`: JWT-backed user/tenant context
- `AccessControlService`: reusable module entitlement and permission evaluation
- `AuditService`: audit logging for auth, school, staff, permission, and plan events
- Audit logs now capture `SchoolId`, value snapshots, and request metadata fields for production-style traceability
- `LocalFileStorageService`: reusable tenant/module-aware file storage abstraction with local implementation

## Day 4 Highlights

- Attendance:
  - class/date session-based marking
  - duplicate prevention by student/day
  - monthly summaries and analytics
- Fees:
  - fee categories, structures, fine rules
  - student/class assignments
  - invoice generation with school-scoped numbering
  - partial payments, pending balance tracking, and export-ready invoice rows
- Quizzes:
  - quiz creation with MCQ questions
  - publish workflow
  - one submission per student
  - auto evaluation plus manual adjustment
  - leaderboard and analytics APIs
- Results:
  - exams with subject-level definitions
  - mark entry and publication
  - student report cards and class result views
- Notice Board:
  - notices and announcements
  - audience targeting
  - publish/unpublish
  - optional attachments
- Communication:
  - parent-teacher conversation threads
  - message history
  - attachment-ready messaging architecture
- Transport:
  - vehicles, routes, drivers
  - student assignments with pickup/drop metadata
- Gallery:
  - albums
  - photo/video uploads
  - tenant-isolated media storage
- ID Cards:
  - versioned template records
  - student and teacher print-ready card payload generation
  - QR/barcode metadata-ready snapshots
- Admit Cards:
  - versioned template records
  - exam-scoped student admit card generation
  - seat/room/instructions metadata
- Parent Portal:
  - parent user provisioning linked to the single `User` authentication model
  - parent-student relation management
  - read-only self-service APIs for attendance, fees, results, homework, and notices
- Dashboard / Reporting:
  - summary cards
  - monthly chart-ready analytics
  - recent activity feed
  - paged export endpoints for students, attendance, fees, and quiz results

## Swagger Notes

- Standard response examples are added globally.
- Authorization notes are included in endpoint descriptions.
- Module-protected endpoints carry module/permission metadata for frontend-friendly discovery.
- Auth endpoints now have rate limiting policies for login, refresh-token, and change-password.

## API Reference

See [API_DOCUMENTATION.md](/e:/Users/Public/dar_al_safat/SchoolERP/API_DOCUMENTATION.md) for endpoint-by-endpoint documentation, request examples, response examples, validation rules, tenant flow, RBAC flow, and subscription entitlement behavior.
