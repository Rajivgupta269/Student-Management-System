# Student-Management-System
# ScholarHub - Smarter Campus. Better Learning.

Student Management System built with **ASP.NET Core 8 MVC, C#, Razor, Entity Framework Core and SQLite**.
Custom HTML/CSS/JavaScript only (no Bootstrap, Tailwind, jQuery, React, Angular or Vue).

## Requirements
- .NET 8 SDK  (https://dotnet.microsoft.com/download)
- VS Code or Visual Studio 2022

## Setup (run inside the ScholarHub folder)
```
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef migrations add InitialCreate
dotnet run
```
Open the https address printed in the terminal.

**First login:** `admin@scholarhub.local` / `Admin@12345` (from `SeedAdmin` in appsettings.json).
Change the password in Settings > My account after the first login.

`dotnet run` applies migrations at startup (it never deletes data) and seeds only what is missing:
2 demo departments, 2 demo courses, default settings, and the admin account.
The database file `scholarhub.db` is created next to the project and survives restarts.

If you change a model later: `dotnet ef migrations add <Name>` then run the app again.
If you ever want a completely fresh database during development: stop the app, delete
`scholarhub.db`, `scholarhub.db-shm`, `scholarhub.db-wal`, and run again.

## Features
| Module | What it does |
|---|---|
| Login | ASP.NET Core Identity (hashed passwords, lockout after 5 failures). Every page needs login. |
| Dashboard | Totals, enrollment chart, attendance doughnut, upcoming exams, recent registrations, quick actions. All from the database. |
| Students | CRUD, search, filter, sort, pagination, duplicate ID check, course/department/semester validation. |
| Courses, Departments, Faculty | CRUD with duplicate-code checks; deleting something that is in use is blocked. |
| Attendance | Mark a whole class for a date (Present/Absent/Late + remarks), re-marking updates instead of duplicating, filters, edit/delete. |
| Examinations | CRUD, upcoming-only filter, passing marks cannot exceed maximum. |
| Results & Marks | Marks validated against the exam maximum, student must belong to the exam's course, one result per student per exam; percentage, grade and pass/fail are calculated. |
| Reports | Enrollment (CSV), department/course counts, attendance %, results (CSV). All print-friendly. |
| Settings | Institution name (footer), students per page, minimum attendance %, change password. |
| REST API | JWT-protected JSON API under `/api` (see below). |

Late counts as attended in attendance percentages.
Deleting a student also deletes that student's attendance and result records (confirmation shown).

## REST API
1. `POST /api/auth/token` with `{ "email": "...", "password": "..." }` returns a JWT.
2. Send `Authorization: Bearer <token>` on every other call.

| Method | Route | Purpose |
|---|---|---|
| GET | /api/stats | Totals and average attendance |
| GET | /api/students?q=&page=&pageSize= | List / search students |
| GET/POST/PUT/DELETE | /api/students[/{id}] | Full student CRUD |
| GET | /api/departments, /api/courses, /api/faculty | Reference lists |
| GET | /api/results/student/{id} | A student's results |

Ready-made requests are in `api-tests.http`. Validation errors return HTTP 400 with details; missing token returns 401.

## Security notes
- Passwords are hashed by Identity. Forms use anti-forgery tokens; all deletes are POST.
- **Before deploying:** change `SeedAdmin:Password` (or remove the `SeedAdmin` block after the first run) and set a new random `Jwt:Key` of 32+ characters. Prefer environment variables over editing appsettings.json on a server, e.g. `Jwt__Key`.
- All signed-in users are administrators (no separate roles).

## Deployment (Phase 4)
```
dotnet publish -c Release -o publish
```
- **Azure App Service / any Windows or Linux host:** upload the `publish` folder, set `ASPNETCORE_ENVIRONMENT=Production`, `Jwt__Key` and `SeedAdmin__Password` as application settings.
- **Database on a server:** SQLite is a single file. Point `ConnectionStrings__DefaultConnection` to a folder that persists between deployments (for Azure App Service use `Data Source=D:\home\data\scholarhub.db`).
- **IIS:** install the .NET 8 Hosting Bundle, create a site pointing to the `publish` folder.
- Run `dotnet ef migrations add ...` on your PC and publish; the app applies migrations at startup.

## Testing checklist
- Visiting any page while logged out redirects to the login page; wrong password shows a generic error.
- Students: add / edit / view / delete; duplicate ID, bad email, 9-digit phone, future DOB, course from another department, semester too high are rejected.
- Courses/Departments: deleting one that is in use is blocked with a message.
- Attendance: mark a class, reload the same course and date (values are pre-filled), change one status and save (no duplicates), edit and delete a record.
- Examinations: passing marks greater than maximum is rejected; Upcoming only filter works.
- Results: marks above maximum rejected; student from a different course rejected; second result for same student and exam rejected; grade and Pass/Fail correct.
- Reports: each report loads, filters work, CSV downloads open in Excel, Print preview hides menus.
- Settings: change students per page and check the Students list; change password and sign in again.
- API: run the requests in `api-tests.http`; a call without a token returns 401.
- Stop and restart the app: all data is still there.
- Resize to phone width: sidebar becomes a drawer, tables scroll sideways.
