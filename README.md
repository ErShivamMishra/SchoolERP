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

- `SchoolManagement`
- `StaffManagement`
- `StudentManagement`
- `TeacherManagement`
- `AttendanceManagement`
- `FeeManagement`
- `ResultManagement`
- `QuizManagement`
- `StudyManagement`
- `IdCardManagement`
- `AdmitCardManagement`

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

- `BASIC` licenses `StaffManagement`, `StudentManagement`, `TeacherManagement`, `AttendanceManagement`
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

## Module Organization

- `Features/Authentication`
- `Features/Schools`
- `Features/Staff`
- `Features/Modules`
- `Features/AccessControl`
- `Features/Subscriptions`

Each feature contains:

- `Models`: request/response DTOs and validators
- `Interfaces`: service/repository contracts
- `Services`: business logic

## Middleware and Cross-Cutting Services

- `GlobalExceptionMiddleware`: standardized API errors
- `SubscriptionValidationMiddleware`: tenant school-state validation plus module/permission enforcement
- `CurrentUserContext`: JWT-backed user/tenant context
- `AccessControlService`: reusable module entitlement and permission evaluation
- `AuditService`: audit logging for auth, school, staff, permission, and plan events

## Swagger Notes

- Standard response examples are added globally.
- Authorization notes are included in endpoint descriptions.
- Module-protected endpoints carry module/permission metadata for frontend-friendly discovery.

## API Reference

See [API_DOCUMENTATION.md](/e:/Users/Public/dar_al_safat/SchoolERP/API_DOCUMENTATION.md) for endpoint-by-endpoint documentation, request examples, response examples, validation rules, tenant flow, RBAC flow, and subscription entitlement behavior.
