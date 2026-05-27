# API Documentation

Base route: `/api/v1`

Response wrapper:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {},
  "errors": []
}
```

## Auth and Tenant Flow

1. SuperAdmin logs in with `POST /api/v1/auth/login`
2. SuperAdmin creates a school with `POST /api/v1/schools`
3. School creation creates:
   - tenant record
   - tenant roles
   - default `SchoolAdmin`
   - default `BASIC` subscription
4. SchoolAdmin logs in with the generated credentials
5. Tenant requests must satisfy:
   - valid JWT
   - active school
   - non-expired subscription
   - licensed module
   - matching permission

JWT usage:

- Header: `Authorization: Bearer <accessToken>`
- Tenant context comes from `SchoolId` / `tenant_id` claims

Swagger usage:

- Development URL: `/swagger`
- JSON URL: `/swagger/v1/swagger.json`

## Day 1 APIs

### `GET /system/health`

- Auth: No
- Purpose: Health check
- Success example:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {
    "status": "ok",
    "utcNow": "2026-05-18T12:00:00Z"
  }
}
```

### `POST /auth/login`

- Auth: No
- Body:

```json
{
  "email": "superadmin@schoolerp.local",
  "password": "SuperAdmin@123"
}
```

- Success data:

```json
{
  "accessToken": "jwt",
  "accessTokenExpiresAtUtc": "2026-05-18T13:00:00Z",
  "refreshToken": "refresh-token",
  "refreshTokenExpiresAtUtc": "2026-05-25T12:00:00Z",
  "user": {
    "id": "guid",
    "schoolId": null,
    "fullName": "Platform Super Admin",
    "email": "superadmin@schoolerp.local",
    "role": "SuperAdmin",
    "isFirstLogin": false
  }
}
```

- Errors:
  - `400`: invalid payload
  - `401`: invalid credentials / locked account
  - `403`: inactive user / suspended school / expired subscription

### `POST /auth/refresh-token`

- Auth: No
- Body:

```json
{
  "refreshToken": "current-refresh-token"
}
```

- Notes:
  - refresh token is rotated on success
  - frontend must replace both tokens

### `POST /auth/change-password`

- Auth: Yes
- Purpose: change a temporary or current password and receive fresh tokens
- Body:

```json
{
  "currentPassword": "Temp@12345",
  "newPassword": "NewStrong@123"
}
```

- Notes:
  - temporary-password users are blocked from other protected APIs until this succeeds
  - login / refresh / change-password endpoints are rate limited

### `POST /schools`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "name": "The City School",
  "code": "CITY001",
  "address": "Main Road",
  "contactEmail": "info@cityschool.com",
  "contactPhone": "+92-300-0000000",
  "subscriptionStartDate": "2026-05-18T00:00:00Z",
  "subscriptionEndDate": "2027-05-17T23:59:59Z",
  "maxStaffLimit": 100
}
```

- Validation:
  - unique `code`
  - valid email
  - `maxStaffLimit > 0`
  - end date later than start date

- Success data includes:
  - `school`
  - `schoolAdminEmail`
  - `temporaryPassword`

### `GET /schools`

- Auth: Yes, `SuperAdmin`
- Purpose: list schools

### `GET /schools/{schoolId}`

- Auth: Yes, `SuperAdmin`
- Purpose: get school details

### `PUT /schools/{schoolId}`

- Auth: Yes, `SuperAdmin`
- Purpose: update school master data

### `PATCH /schools/{schoolId}/activation`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "isActive": false
}
```

### `PATCH /schools/{schoolId}/subscription/extend`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "newSubscriptionEndDate": "2027-12-31T23:59:59Z"
}
```

## Day 2 APIs

## Staff Management

Role access:

- `SuperAdmin`: any school
- `SchoolAdmin`: current school only

Module requirement:

- `StaffManagement`

### `POST /staff`

- Auth: Yes
- Body:

```json
{
  "schoolId": "school-guid",
  "fullName": "Ayesha Khan",
  "email": "ayesha@cityschool.com",
  "phoneNumber": "+92-300-1111111",
  "password": "Temp@12345",
  "roleId": "role-guid"
}
```

- Validation:
  - `schoolId` required only for `SuperAdmin`
  - duplicate email blocked within same tenant
  - school staff limit enforced
  - `SchoolAdmin` cannot assign `SuperAdmin`

- Success data:

```json
{
  "id": "staff-guid",
  "schoolId": "school-guid",
  "fullName": "Ayesha Khan",
  "email": "ayesha@cityschool.com",
  "phoneNumber": "+92-300-1111111",
  "roleId": "role-guid",
  "roleName": "Staff",
  "isActive": true,
  "isFirstLogin": true,
  "createdAt": "2026-05-18T12:00:00Z"
}
```

### `PUT /staff/{staffId}`

- Auth: Yes
- Body:

```json
{
  "fullName": "Ayesha Khan Updated",
  "email": "ayesha@cityschool.com",
  "phoneNumber": "+92-300-2222222",
  "roleId": "role-guid"
}
```

### `DELETE /staff/{staffId}`

- Auth: Yes
- Behavior: soft delete

### `PATCH /staff/{staffId}/activation`

- Auth: Yes
- Body:

```json
{
  "isActive": true
}
```

### `POST /staff/{staffId}/reset-password`

- Auth: Yes
- Success data:

```json
{
  "staffId": "staff-guid",
  "temporaryPassword": "Temp@abc123xyz"
}
```

### `GET /staff/{staffId}`

- Auth: Yes
- Purpose: fetch one staff member

### `GET /staff`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `pageNumber`
  - `pageSize`
  - `search`
  - `roleId`
  - `isActive`

- Success data:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

## RBAC and User Permissions

Permission flow:

1. Role permissions define the default access baseline
2. User permissions can be assigned per module
3. Effective access also requires plan entitlement
4. `SuperAdmin` bypasses permission and licensing checks

### `PUT /permissions/users/{userId}`

- Auth: Yes
- Body:

```json
{
  "permissions": [
    {
      "moduleId": "module-guid",
      "canView": true,
      "canCreate": true,
      "canEdit": false,
      "canDelete": false
    }
  ]
}
```

- Role access:
  - `SuperAdmin`: any user
  - `SchoolAdmin`: same-tenant users only

### `GET /permissions/users/{userId}`

- Auth: Yes
- Purpose: fetch user-specific module permissions

### `GET /permissions/roles/{roleId}`

- Auth: Yes
- Purpose: fetch role defaults grouped by module

### `GET /permissions/modules`

- Auth: Yes
- Purpose: fetch modules for permission-assignment screens

## Module Management

Role access:

- `SuperAdmin` only for create/update/activation

### `POST /modules`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "name": "Library Management",
  "code": "LibraryManagement",
  "description": "Library operations",
  "displayOrder": 120
}
```

### `PUT /modules/{moduleId}`

- Auth: Yes, `SuperAdmin`

### `PATCH /modules/{moduleId}/activation`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "isActive": true
}
```

### `GET /modules`

- Auth: No
- Purpose: public module list for discovery or bootstrap screens

## Subscription Plans and Module Entitlement

Subscription flow:

1. Create plan
2. Assign licensed modules to plan
3. Assign plan to school
4. Tenant access is denied when module is not licensed, even if permission exists

### `POST /subscription-plans`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "name": "Enterprise",
  "code": "ENTERPRISE",
  "price": 4999
}
```

### `PUT /subscription-plans/{planId}/modules`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "modules": [
    {
      "moduleId": "module-guid",
      "isLicensed": true,
      "isVisibleInMenu": true
    }
  ]
}
```

### `POST /schools/{schoolId}/subscription-plan`

- Auth: Yes, `SuperAdmin`
- Body:

```json
{
  "subscriptionPlanId": "plan-guid",
  "startDate": "2026-05-18T00:00:00Z",
  "endDate": "2027-05-17T23:59:59Z"
}
```

## Standard Error Responses

Validation example:

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": [
    "Email is required.",
    "Role Id is required."
  ]
}
```

Forbidden example:

```json
{
  "success": false,
  "message": "You do not have permission to access this module.",
  "errors": []
}
```

Locked module example:

```json
{
  "success": false,
  "message": "You do not have permission to access this module.",
  "errors": []
}
```

Cross-tenant example:

```json
{
  "success": false,
  "message": "Staff access is limited to the current school.",
  "errors": []
}
```

## Validation and Access Summary

- Duplicate emails are tenant-scoped
- Staff quota uses `School.MaxStaffLimit`
- `SchoolAdmin` cannot manage another school
- `SchoolAdmin` cannot create `SuperAdmin`
- Tenant requests require both permission and plan entitlement
- Existing auth token flow is unchanged

## Day 3 APIs

## Admission Management

Module requirement:

- `AdmissionManagement`

### `POST /admissions/academic-sessions`

- Auth: Yes
- Purpose: create academic session

### `GET /admissions/academic-sessions`

- Auth: Yes
- Purpose: list academic sessions for the tenant

### `POST /admissions/classes`

- Auth: Yes
- Purpose: create class master

### `GET /admissions/classes`

- Auth: Yes
- Purpose: list class masters

### `POST /admissions/sections`

- Auth: Yes
- Purpose: create section under a class

### `GET /admissions/sections`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `classId` optional

### `POST /admissions`

- Auth: Yes
- Purpose: create admission application
- Validation:
  - `admissionNumber` unique inside tenant
  - duplicate mobile/email blocked inside tenant
  - class and academic session must belong to the tenant

### `PUT /admissions/{admissionId}`

- Auth: Yes
- Purpose: update admission before conversion

### `PATCH /admissions/{admissionId}/approve`

- Auth: Yes
- Purpose: approve admission

### `PATCH /admissions/{admissionId}/reject`

- Auth: Yes
- Purpose: reject admission

### `GET /admissions/{admissionId}`

- Auth: Yes
- Purpose: fetch one admission

### `GET /admissions`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `pageNumber`
  - `pageSize`
  - `search`
  - `status`
  - `appliedClassId`
  - `academicSessionId`

## Student Management

Module requirement:

- `StudentManagement`

### `POST /students/from-admission`

- Auth: Yes
- Purpose: convert approved admission into student
- Validation:
  - admission must be approved
  - admission cannot be converted twice
  - roll number unique inside class/section/session

### `POST /students`

- Auth: Yes
- Purpose: create student manually

### `PUT /students/{studentId}`

- Auth: Yes
- Purpose: update student

### `GET /students/{studentId}`

- Auth: Yes
- Purpose: fetch one student with uploaded documents

### `GET /students`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `pageNumber`
  - `pageSize`
  - `search`
  - `classId`
  - `sectionId`
  - `academicSessionId`
  - `isActive`

### `PATCH /students/{studentId}/promote`

- Auth: Yes
- Purpose: promote student to new class/section/session

### `PATCH /students/{studentId}/transfer`

- Auth: Yes
- Purpose: transfer student to new class/section/session

### `PATCH /students/{studentId}/deactivate`

- Auth: Yes
- Purpose: deactivate student

### `POST /students/{studentId}/documents`

- Auth: Yes
- Content type: `multipart/form-data`
- Purpose: upload student document
- Validation:
  - allowed types: PDF, JPG, PNG, DOC, DOCX
  - max size: 5 MB

## Teacher Management

Module requirement:

- `TeacherManagement`

### `POST /teachers`

- Auth: Yes
- Purpose: create teacher
- Validation:
  - `employeeCode` unique inside tenant

### `PUT /teachers/{teacherId}`

- Auth: Yes
- Purpose: update teacher

### `PUT /teachers/{teacherId}/subjects`

- Auth: Yes
- Purpose: replace teacher subject assignments

### `PUT /teachers/{teacherId}/classes`

- Auth: Yes
- Purpose: replace teacher class assignments

### `GET /teachers/{teacherId}`

- Auth: Yes
- Purpose: fetch one teacher with subjects and classes

### `GET /teachers`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `pageNumber`
  - `pageSize`
  - `search`
  - `isActive`

### `PATCH /teachers/{teacherId}/deactivate`

- Auth: Yes
- Purpose: deactivate teacher

## Study Management

Module requirement:

- `StudyManagement`

### `POST /study/subjects`

- Auth: Yes
- Purpose: create subject

### `PUT /study/subjects/{subjectId}`

- Auth: Yes
- Purpose: update subject

### `GET /study/subjects`

- Auth: Yes
- Purpose: list subjects

### `POST /study/syllabi`

- Auth: Yes
- Purpose: create or update syllabus by subject/class/session

### `POST /study/materials`

- Auth: Yes
- Content type: `multipart/form-data`
- Purpose: upload study material
- Validation:
  - allowed types: PDF, JPG, PNG, DOCX, PPTX
  - max size: 10 MB

### `POST /study/homework`

- Auth: Yes
- Content type: `multipart/form-data`
- Purpose: create homework assignment with optional attachment

### `GET /study/materials`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `classId` optional
  - `subjectId` optional

### `GET /study/homework`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `classId`
  - `sectionId` optional

## File Upload Notes

- Uploaded files are organized by tenant, module, and category under `/uploads/...`
- Storage implementation is abstracted behind a reusable file service for future Azure Blob / S3-compatible providers
- Frontend should submit upload requests as `multipart/form-data`

## Access Notes For Day 3

- Tenant users must rely on JWT-derived `SchoolId`; cross-tenant request payloads are rejected
- `SuperAdmin` can target any school by providing `schoolId` where required
- Module entitlement and RBAC checks apply to all Day 3 APIs before business logic executes

## Day 4 APIs

## Attendance Management

Module requirement:

- `AttendanceManagement`

### `POST /attendance/mark`

- Auth: Yes
- Purpose: mark attendance for a class/section/day
- Validation:
  - one attendance session per class/section/day
  - one attendance record per student/day
  - student must belong to the tenant/class/section

### `POST /attendance/bulk`

- Auth: Yes
- Purpose: bulk attendance insert for the same session payload

### `PUT /attendance/records/{attendanceRecordId}`

- Auth: Yes
- Purpose: update an attendance record and refresh monthly summary

### `GET /attendance/daily-report`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `attendanceDate`
  - `classId` optional
  - `sectionId` optional
  - `pageNumber`
  - `pageSize`

### `GET /attendance/students/{studentId}/history`

- Auth: Yes
- Purpose: paged attendance history for one student

### `GET /attendance/monthly-summary`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `year`
  - `month`
  - `classId` optional
  - `sectionId` optional

### `GET /attendance/analytics`

- Auth: Yes
- Purpose: attendance trend and aggregate counts

## Fee Management

Module requirement:

- `FeeManagement`

### `POST /fees/categories`

- Auth: Yes
- Purpose: create fee category

### `POST /fees/structures`

- Auth: Yes
- Purpose: create fee structure for tenant/class/section scope

### `POST /fees/fine-rules`

- Auth: Yes
- Purpose: create configurable late fine rule

### `POST /fees/assignments`

- Auth: Yes
- Purpose: assign fee structure to selected students or a class/section

### `POST /fees/invoices`

- Auth: Yes
- Purpose: generate invoice with optional installments
- Business rules:
  - invoice number is unique per school
  - pending amount is tracked automatically

### `POST /fees/invoices/{invoiceId}/payments`

- Auth: Yes
- Purpose: record payment transaction
- Business rules:
  - partial payment supported
  - overpayment blocked
  - invoice status recalculated automatically

### `GET /fees/invoices`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `studentId` optional
  - `status` optional
  - `fromDate` / `toDate` optional
  - `search`
  - `pageNumber`
  - `pageSize`

### `GET /fees/students/{studentId}/payments`

- Auth: Yes
- Purpose: payment history per student

### `GET /fees/analytics`

- Auth: Yes
- Purpose: pending fees, overdue, collected amount, monthly revenue

### `GET /fees/exports/invoices`

- Auth: Yes
- Purpose: export-ready paged invoice rows

## Quiz Management

Module requirement:

- `QuizManagement`

### `POST /quizzes`

- Auth: Yes
- Purpose: create quiz with questions and options

### `PATCH /quizzes/{quizId}/publish`

- Auth: Yes
- Purpose: publish or unpublish quiz

### `POST /quizzes/{quizId}/students/{studentId}/submit`

- Auth: Yes
- Purpose: submit quiz answers
- Business rules:
  - only within quiz window
  - one submission per student
  - auto evaluation on submission

### `PATCH /quizzes/{quizId}/students/{studentId}/evaluate`

- Auth: Yes
- Purpose: manual score adjustment and evaluator remarks

### `GET /quizzes`

- Auth: Yes
- Query:
  - `schoolId` for `SuperAdmin`
  - `classId`, `sectionId`, `subjectId` optional
  - `isPublished` optional
  - `search`
  - `pageNumber`
  - `pageSize`

### `GET /quizzes/{quizId}`

- Auth: Yes
- Purpose: fetch quiz with questions

### `GET /quizzes/{quizId}/leaderboard`

- Auth: Yes
- Purpose: ranking API for frontend leaderboard screens

### `GET /quizzes/analytics`

- Auth: Yes
- Purpose: quiz participation and average score analytics

## Dashboard, Audit, and Reporting

Module requirement:

- `DashboardManagement`

### `GET /dashboard/summary`

- Auth: Yes
- Purpose: summary cards for platform or tenant dashboards

### `GET /dashboard/analytics`

- Auth: Yes
- Purpose: chart-ready admissions, revenue, and attendance trends

### `GET /dashboard/recent-activity`

- Auth: Yes
- Purpose: recent audit-backed activity feed

### `GET /audit-logs`

- Auth: Yes, `SuperAdmin` or `SchoolAdmin`
- Purpose: immutable audit log listing with filters for user, module, and action

### `GET /reports/exports/students`

### `GET /reports/exports/attendance`

### `GET /reports/exports/fees`

### `GET /reports/exports/quiz-results`

- Auth: Yes
- Purpose: paged export-ready DTOs for CSV/Excel/PDF pipelines

## Day 4 Validation and Access Summary

- All new APIs remain under `/api/v1`
- All new controllers use DTO-only request/response contracts
- Tenant users are scoped by JWT-derived `SchoolId`
- `SuperAdmin` can query cross-tenant data by providing `schoolId`
- Module entitlement checks still run through the existing subscription middleware
- RBAC still runs through `ModuleAccess` plus role/user permission evaluation
- Audit logs now store change snapshots and request metadata fields
- Pagination responses now include `totalPages`, `hasNextPage`, and `hasPreviousPage`

## Final Coverage Additions

## Exam and Result Management

Module requirement:

- `ResultManagement`

### `POST /results/exams`

- Auth: Yes
- Purpose: create exam definition with class/session scope and subject-wise marks

### `PATCH /results/exams/{examId}/publish`

- Auth: Yes
- Purpose: publish or unpublish exam

### `POST /results/exams/{examId}/records`

- Auth: Yes
- Purpose: create or update exam result entries
- Validation:
  - obtained marks cannot exceed max marks
  - result entries must stay within exam tenant/class scope

### `GET /results/exams`

### `GET /results/exams/{examId}`

### `GET /results/exams/{examId}/students/{studentId}`

### `GET /results/exams/{examId}/class-results`

### `GET /results/analytics`

- Auth: Yes
- Purpose: exam discovery, report cards, class results, and result analytics

## Notice Board and Announcements

Module requirement:

- `NoticeBoardManagement`

### `POST /notices`

- Auth: Yes
- Content type: `multipart/form-data`
- Purpose: create notice or announcement with optional attachment and audience targeting

### `PATCH /notices/{noticeId}/publish`

### `GET /notices`

### `GET /notices/{noticeId}`

- Auth: Yes
- Purpose: publish/unpublish and list notices

## Parent-Teacher Communication

Module requirement:

- `CommunicationManagement`

### `POST /communications/conversations`

- Auth: Yes
- Purpose: create teacher-parent/student communication thread

### `POST /communications/conversations/{conversationId}/messages`

- Auth: Yes
- Content type: `multipart/form-data`
- Purpose: send message in conversation with future notification-ready structure

### `GET /communications/conversations`

### `GET /communications/conversations/{conversationId}/messages`

- Auth: Yes
- Purpose: conversation list and communication history

## Transport Management

Module requirement:

- `TransportManagement`

### `POST /transport/vehicles`

### `POST /transport/routes`

### `POST /transport/drivers`

### `POST /transport/assignments`

### `GET /transport/vehicles`

### `GET /transport/routes`

### `GET /transport/drivers`

### `GET /transport/assignments`

- Auth: Yes
- Purpose: manage vehicles, routes, drivers, and student transport assignments

## Photo and Video Gallery

Module requirement:

- `GalleryManagement`

### `POST /gallery/albums`

### `PATCH /gallery/albums/{albumId}/publish`

### `POST /gallery/albums/{albumId}/media`

### `GET /gallery/albums`

### `GET /gallery/albums/{albumId}/media`

- Auth: Yes
- Purpose: album management and tenant-isolated photo/video uploads

## ID Card Management

Module requirement:

- `IdCardManagement`

### `POST /id-cards/templates`

- Auth: Yes
- Purpose: create a versioned ID card template

### `POST /id-cards/generate/students`

- Auth: Yes
- Purpose: bulk-generate print-ready student ID card payloads

### `POST /id-cards/generate/teachers`

- Auth: Yes
- Purpose: bulk-generate print-ready teacher ID card payloads

## Admit Card Management

Module requirement:

- `AdmitCardManagement`

### `POST /admit-cards/templates`

- Auth: Yes
- Purpose: create a versioned admit card template

### `POST /admit-cards/generate`

- Auth: Yes
- Purpose: generate exam-wise student admit cards with seat and room metadata
- Business rules:
  - student must belong to the exam class/section scope
  - admit card generation is tenant-scoped

## Parent Portal Support

Module requirement:

- `ParentPortalManagement`

### `POST /parents`

- Auth: Yes, `SuperAdmin` / `SchoolAdmin` / `Staff`
- Purpose: create a parent user profile linked to the single `User` authentication model
- Success data includes:
  - `parent`
  - `temporaryPassword`

### `POST /parents/{parentId}/students`

- Auth: Yes, `SuperAdmin` / `SchoolAdmin` / `Staff`
- Purpose: link a parent to a student with view permissions

### `GET /parents/me/students`

- Auth: Yes, `Parent`
- Purpose: list students linked to the current parent account

### `GET /parents/me/students/{studentId}/attendance`

### `GET /parents/me/students/{studentId}/fees`

### `GET /parents/me/students/{studentId}/results`

### `GET /parents/me/students/{studentId}/homework`

### `GET /parents/me/notices`

- Auth: Yes, `Parent`
- Purpose: read-only parent portal APIs
- Business rules:
  - parent can access linked students only
  - tenant isolation is enforced from the authenticated parent account

## Feature Coverage Summary

- Student Information Management: covered by `Students` plus admission conversion and student documents.
- Staff & Teacher Management: covered by `Staff` and `Teachers`.
- Attendance Management: covered by `Attendance`.
- Exam & Result Management: covered by `Quizzes` and direct `Results`.
- Fee & Invoice Management: covered by `Fees`.
- Online Admission Management: covered by `Admissions`.
- Homework & Assignment Management: covered by `Study`.
- Notice Board & Announcements: covered by `Notices`.
- Parent-Teacher Communication: covered by `Communications`.
- Reports & Analytics Dashboard: covered by `Dashboard`, `AuditLogs`, and `Reports`.
- Transport Management: covered by `Transport`.
- Photo & Video Gallery: covered by `Gallery`.
