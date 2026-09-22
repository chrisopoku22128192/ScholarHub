does ur # ScholarHub

An academic resource sharing platform for university students — students can upload, search, and download verified past questions, lecture notes, and slides, organised by department, course code, and academic year. Full details in `ScholarHub_NET_Proposal.pdf`.

## Stack

- ASP.NET Core 8 Razor Pages (server-rendered UI)
- ASP.NET Core Web API (`/api/resources`) — a REST surface over the same service layer the UI uses
- ASP.NET Core Identity (cookie auth, `Admin`/`Student` roles)
- Entity Framework Core with SQLite (`app.db`, local file for dev; swap to SQL Server by changing `ConnectionStrings:DefaultConnection` and the `UseSqlite`/`UseSqlServer` call in `Program.cs`)
- Local disk file storage (`App_Data/Uploads/`, outside `wwwroot` so files are never directly link-able — every download goes through the authorized download handler)

## Running it

```
cd src/ScholarHub.Web
dotnet run
```

On first run this automatically applies EF Core migrations and seeds the `Admin`/`Student` roles plus a default admin account:

- **Email:** `admin@scholarhub.local`
- **Password:** `Admin#12345`

Override those via `Seed:AdminEmail` / `Seed:AdminPassword` in `appsettings.Development.json` or user secrets before the first run if you want different defaults.

Anyone can register a student account from the site; there's no self-serve admin signup by design — promote a user by adding them to the `Admin` role directly (e.g. via a quick EF Core/SQL script), matching a real moderation model.

In Development, Swagger UI for the REST API is available at `/swagger`.

## Project layout

- `Models/` — `Resource`, `ApplicationUser`, enums, `Departments` lookup list
- `Data/` — `ApplicationDbContext`, migrations, `SeedData` (roles + default admin)
- `Services/` — `IResourceService`/`ResourceService` (search, upload, moderation, download) and `IFileStorageService`/`LocalFileStorageService`, shared by both the Razor Pages UI and the API controller
- `Pages/` — resource library (`Index`), `Upload`, `MyUploads`, `Resources/Details`, `Resources/Download`, `Admin/Index` (moderation console)
- `Controllers/ResourcesController.cs` — REST API (`GET/POST /api/resources`, `/mine`, `/pending`, `/{id}/approve`, `/{id}/reject`, `/{id}/download`)

## Team

| # | Name | Role |
|---|------|------|
| 1 | Austion Bediako - 22126218 | Team Lead / Full-Stack Developer |
| 2 | Kenny Idan - 22180114 | Backend Developer (ASP.NET Core API) |
| 3 | Chris Nana Opoku - 22128192 | Frontend Developer (Blazor / Razor Pages) |
| 4 | Joseph Akondoh-Tetteh - 22055467 | Database Engineer (EF Core / SQL Server) |
| 5 | Noble Ackah-Yensu - 22047433 | Authentication & Security (ASP.NET Identity) |
| 6 | Robert Owoo - 22018250 | File Upload & Storage Module |
| 7 | Nicole Eshun - 22241518 | Search & Filter Implementation |
| 8 | Gabriel Ansah - 22018785 | UI/UX Designer |
| 9 | Kwabena Yeboah - 22150775 | Admin Panel Developer |
| 10 | Bamanjo Angela - 22077960 | QA & Testing Engineer |
| 11 | Blessing Owusu Frema - 22108048 | Documentation & Report Writing |
| 12 | Aiden Be-ir - 22034367 | DevOps / Deployment |

## Notes

- The API shares the same Identity cookie as the Razor Pages UI rather than a separate token scheme — simplest for a single deployed app. A public/mobile client would need bearer-token auth added instead.
- Uploads accept PDF, DOCX, and PPTX up to 25 MB (see `LocalFileStorageService`).
- New uploads are always `Pending` until an admin approves or rejects them from `/Admin`.
