# SchoolERP API Flow Guide for UI Developers

This README is for frontend and UI developers who will start building screens on top of the APIs that are available in `SchoolERP` right now.

The goal is simple: understand which API should be called first, what each built API does, which role can call it, and what flow the UI should follow.

This document is based on:

- the APIs currently implemented in the codebase
- the updated V2 SRS/FRS direction
- the current backend behavior as of the existing `v1` controllers

## Current API Modules Built

As of now, these API areas are implemented:

- `System`: health check
- `Auth`: login and refresh token
- `Schools`: school onboarding and school lifecycle management

These larger modules are mentioned in SRS/FRS but are **not built yet** in API form:

- forgot password / reset password / change password
- subscription purchase or plan upgrade APIs
- campus, students, staff, admissions, fees, exams, reports, notifications
- school-admin user management APIs

## Main UI Flow

For the UI team, the working flow right now is:

1. Super Admin logs in.
2. Super Admin creates a school.
3. `Create School` automatically creates:
   - the school record
   - a default `SchoolAdmin` user
   - a temporary password for that school admin
   - a default `BASIC` subscription
4. Super Admin can view, update, activate/deactivate, and extend subscription for the school.
5. School Admin can then log in by using the generated email and temporary password returned from `Create School`.
6. After School Admin login, UI should treat `user.isFirstLogin = true` as a signal to send the user to a future "set new password" screen.

Important: the backend currently marks first login, but **there is no built API yet to set/change/reset password**. So for now the UI can show the first-login state, but the actual password-reset flow cannot be completed until that API is added.

## Roles You Need to Know

- `SuperAdmin`: platform-level user. Can log in and manage schools.
- `SchoolAdmin`: tenant-level admin automatically created when a school is created.
- `Staff`: role exists, but staff-facing APIs are not implemented yet.

## Base URL

Development launch settings show:

- `http://localhost:5049`
- `https://localhost:7072`

API version prefix used now:

- `/api/v1`

Examples:

- `POST /api/v1/auth/login`
- `POST /api/v1/schools`

## Default Super Admin Login

The app seeds one platform super admin from configuration.

- Email: `superadmin@schoolerp.local`
- Password: `SuperAdmin@123`

Use this account first to start the onboarding flow from the UI.

## Authentication Rules

- Login returns `accessToken` and `refreshToken`.
- Protected APIs require `Authorization: Bearer <accessToken>`.
- `Schools` APIs are restricted to `SuperAdmin`.
- `SuperAdmin` is not blocked by school subscription rules.
- Tenant users like `SchoolAdmin` are blocked if:
  - the school is inactive
  - the school is suspended
  - the subscription is expired

## Standard Response Shape

Every API uses this general shape:

```json
{
  "success": true,
  "message": "Human readable message",
  "data": {},
  "errors": []
}
```

Validation and business errors also return the same wrapper with `success = false`.

## Built API Reference

### 1. System Health

#### `GET /api/v1/system/health`

Purpose:
Check whether the API is running.

Auth:
No login required.

UI usage:
Useful for initial connectivity check or admin diagnostics page.

Success response data:

```json
{
  "status": "ok",
  "utcNow": "2026-05-12T12:00:00Z"
}
```

### 2. Login

#### `POST /api/v1/auth/login`

Purpose:
Authenticate a user and return JWT access/refresh tokens.

Auth:
Public API.

Request body:

```json
{
  "email": "superadmin@schoolerp.local",
  "password": "SuperAdmin@123"
}
```

UI usage:

- first API for Super Admin login page
- later also used by School Admin login page

Important behavior:

- invalid credentials return an auth error
- inactive user cannot log in
- locked user cannot log in
- School Admin login is blocked if school subscription is expired or suspended

Success response data shape:

```json
{
  "accessToken": "jwt-access-token",
  "accessTokenExpiresAtUtc": "2026-05-12T13:00:00Z",
  "refreshToken": "plain-refresh-token",
  "refreshTokenExpiresAtUtc": "2026-05-19T12:00:00Z",
  "user": {
    "id": "user-guid",
    "schoolId": "school-guid-or-null",
    "fullName": "Platform Super Admin",
    "email": "superadmin@schoolerp.local",
    "role": "SuperAdmin",
    "isFirstLogin": false
  }
}
```

UI notes:

- store `accessToken` for authorized API calls
- store `refreshToken` for silent session renewal
- use `user.role` to drive routing
- use `user.isFirstLogin` to show first-login password setup flow later

### 3. Refresh Token

#### `POST /api/v1/auth/refresh-token`

Purpose:
Generate a new access token and refresh token pair when the access token expires.

Auth:
Public API, but requires a valid refresh token.

Request body:

```json
{
  "refreshToken": "current-refresh-token"
}
```

UI usage:

- call when access token expires
- update stored access token and refresh token from the new response

Important behavior:

- refresh token is rotated on each successful refresh
- old refresh token should be replaced in frontend storage

### 4. Create School

#### `POST /api/v1/schools`

Purpose:
Create a new school tenant and bootstrap its initial admin access.

Auth:
Requires `SuperAdmin` token.

This is the most important API for UI onboarding flow.

What this API does internally:

- creates the school
- creates tenant roles for that school
- creates a default `SchoolAdmin`
- generates a temporary password
- creates a default `BASIC` subscription
- marks the school active

Request body:

```json
{
  "name": "The City School",
  "code": "CITY001",
  "address": "Main Road, Karachi",
  "contactEmail": "info@cityschool.com",
  "contactPhone": "+92-300-0000000",
  "subscriptionStartDate": "2026-05-12T00:00:00Z",
  "subscriptionEndDate": "2027-05-11T23:59:59Z",
  "maxStaffLimit": 100
}
```

Validation rules:

- `name` is required
- `code` is required and must be unique
- `code` allows only letters, numbers, `_` and `-`
- `address` is required
- `contactEmail` must be valid
- `contactPhone` is required
- `maxStaffLimit` must be greater than `0`
- `subscriptionEndDate` must be after `subscriptionStartDate`

Success response data shape:

```json
{
  "school": {
    "id": "school-guid",
    "name": "The City School",
    "code": "CITY001",
    "address": "Main Road, Karachi",
    "contactEmail": "info@cityschool.com",
    "contactPhone": "+92-300-0000000",
    "subscriptionStartDate": "2026-05-12T00:00:00Z",
    "subscriptionEndDate": "2027-05-11T23:59:59Z",
    "maxStaffLimit": 100,
    "isActive": true,
    "createdAt": "2026-05-12T12:00:00Z"
  },
  "schoolAdminEmail": "admin@city001.schoolerp.local",
  "temporaryPassword": "Temp@abc123..."
}
```

UI usage:

- used by Super Admin "Create School" screen
- after success, show:
  - school created successfully
  - generated school admin email
  - generated temporary password
- UI should allow copy/view of these credentials because they are needed for School Admin login

Important note for your question about "create password for that school":

- there is **no separate create-password API yet**
- the password is auto-generated by `Create School`
- that temporary password is returned in the response
- School Admin uses that temporary password to log in first time

### 5. Get All Schools

#### `GET /api/v1/schools`

Purpose:
Return all schools created so far.

Auth:
Requires `SuperAdmin` token.

UI usage:

- Super Admin school listing page
- dashboard table of all tenants

Response:
Returns an array of `SchoolDto`.

### 6. Get School By Id

#### `GET /api/v1/schools/{schoolId}`

Purpose:
Return one school by id.

Auth:
Requires `SuperAdmin` token.

UI usage:

- school details page
- prefill edit form

### 7. Update School

#### `PUT /api/v1/schools/{schoolId}`

Purpose:
Update school master information.

Auth:
Requires `SuperAdmin` token.

Request body:

```json
{
  "name": "The City School Updated",
  "address": "New Address",
  "contactEmail": "admin@cityschool.com",
  "contactPhone": "+92-300-0000001",
  "maxStaffLimit": 150
}
```

UI usage:

- edit school screen
- update contact details and staff quota

Important behavior:

- school `code` cannot be updated through this API
- subscription dates are not updated through this API

### 8. Activate or Deactivate School

#### `PATCH /api/v1/schools/{schoolId}/activation`

Purpose:
Enable or disable a school.

Auth:
Requires `SuperAdmin` token.

Request body:

```json
{
  "isActive": false
}
```

UI usage:

- active/inactive toggle in school management
- suspend tenant access from platform side

Important behavior:

- `isActive = false` makes the school suspended/inactive
- inactive schools cannot be used by tenant users like `SchoolAdmin`

### 9. Extend School Subscription

#### `PATCH /api/v1/schools/{schoolId}/subscription/extend`

Purpose:
Extend the school subscription end date.

Auth:
Requires `SuperAdmin` token.

Request body:

```json
{
  "newSubscriptionEndDate": "2027-12-31T23:59:59Z"
}
```

UI usage:

- subscription renewal screen
- admin action after offline renewal or manual renewal

Important behavior:

- new date must be later than current subscription end date
- backend updates both school-level subscription dates and latest subscription record

Important limitation:

- there is no separate payment or plan-selection API yet
- there is no `upgrade from BASIC to PREMIUM` API yet
- current renewal behavior is manual date extension by Super Admin

## Recommended UI Screen Flow

### Platform Side

1. Login screen for `SuperAdmin`
2. School listing screen
3. Create school screen
4. School details screen
5. Edit school screen
6. Subscription extension action
7. Activation/deactivation action

### Tenant Side

1. Login screen for `SchoolAdmin`
2. First-login info screen
3. Placeholder "set new password" screen

Important:
Because password reset APIs are not yet implemented, the tenant-side first-login flow can only be prepared in UI for now.

## Suggested Frontend Logic

### After Super Admin Login

- call `POST /api/v1/auth/login`
- store token pair
- route to school management area

### After Create School

- show success modal or details card
- show `schoolAdminEmail`
- show `temporaryPassword`
- optionally provide copy buttons
- optionally provide "Go to school list" action

### After School Admin Login

- if `user.isFirstLogin` is `true`, route to first-login password setup UI
- show message like: "Your account was created by platform admin. Password setup API is pending in backend."

### Token Refresh

- when API returns unauthorized because access token expired, call refresh-token API
- replace both old tokens with new ones

## Error Cases UI Should Handle

- invalid login credentials
- locked account after repeated failed login
- inactive user
- super-admin-only access denied
- duplicate school code
- invalid school payload
- school not found
- expired subscription for tenant login/use
- inactive or suspended school

## Important Backend Notes for UI Team

### SchoolAdmin Email Format

When a school is created, backend generates School Admin email like this:

- `admin@{schoolCodeLowercase}.schoolerp.local`

Example:

- school code `CITY001`
- generated email `admin@city001.schoolerp.local`

UI should not ask the user to manually enter this unless product wants to display/edit it later through a future API.

### Subscription Behavior Right Now

Current backend behavior is simple:

- every new school gets default `BASIC` plan
- subscription is active immediately
- tenant access stops when school is inactive or subscription expires
- Super Admin can manually extend subscription

### Password Behavior Right Now

Current backend behavior:

- school admin temporary password is generated during school creation
- user is marked as first login / requires password reset
- login is still allowed with temporary password
- reset/set-password API is not built yet

So the UI should reflect the intended flow, but also clearly know this gap exists in current backend implementation.

## What Is Built vs Planned

### Built and usable now

- health check
- Super Admin login
- token refresh
- create school
- list schools
- get school by id
- update school
- activate/deactivate school
- extend subscription
- School Admin login with generated credentials

### Planned by SRS/FRS but not yet built

- password setup / forgot password / change password
- plan upgrade or premium subscription purchase
- module entitlement APIs
- school user management
- staff, student, academic, fee, exam, report, notification modules

## Quick API Table

| API | Method | Who uses it | Purpose |
|---|---|---|---|
| `/api/v1/system/health` | `GET` | Public | Check API status |
| `/api/v1/auth/login` | `POST` | SuperAdmin, SchoolAdmin | Login and get tokens |
| `/api/v1/auth/refresh-token` | `POST` | Logged-in users | Renew token pair |
| `/api/v1/schools` | `POST` | SuperAdmin | Create school, school admin, temp password, default subscription |
| `/api/v1/schools` | `GET` | SuperAdmin | List all schools |
| `/api/v1/schools/{schoolId}` | `GET` | SuperAdmin | Get school details |
| `/api/v1/schools/{schoolId}` | `PUT` | SuperAdmin | Update school details |
| `/api/v1/schools/{schoolId}/activation` | `PATCH` | SuperAdmin | Activate/deactivate school |
| `/api/v1/schools/{schoolId}/subscription/extend` | `PATCH` | SuperAdmin | Extend subscription |

## Best Starting Point for the UI Team

If a UI developer wants the correct working order, use this exact sequence:

1. Call `POST /api/v1/auth/login` with Super Admin credentials.
2. Save the access token.
3. Call `POST /api/v1/schools` to create a school.
4. Save or display the returned `school.id`, `schoolAdminEmail`, and `temporaryPassword`.
5. Use `GET /api/v1/schools` and `GET /api/v1/schools/{schoolId}` for school listing/detail screens.
6. Use `PUT /api/v1/schools/{schoolId}` for edit screen.
7. Use `PATCH /api/v1/schools/{schoolId}/activation` for active/inactive toggle.
8. Use `PATCH /api/v1/schools/{schoolId}/subscription/extend` for renewal flow.
9. Use the returned school admin credentials to test tenant login with `POST /api/v1/auth/login`.

## Final Clarification

For the specific flow you asked:

- first login as `SuperAdmin`
- then call `Create School`
- that API already creates:
  - school
  - school admin account
  - temporary password
  - default subscription
- there is currently no separate API to create/set password for that school admin

So for UI design, assume:

- onboarding starts from Super Admin
- tenant credentials are produced by backend during school creation
- first-login password-change flow is a planned screen, but backend API for it is still pending
