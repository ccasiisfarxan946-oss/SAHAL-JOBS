# SAHAL-JOBS

# Laboure Skills Platform (LSP) — Final Year Project

An ASP.NET Core 9 Razor Pages application implementing the system described in
Chapters One, Two and Three of the accompanying thesis: a three-role (Admin,
Employer, Labourer) labour-marketplace with skill verification and a
multi-criteria job-matching engine.

## Technology Stack (exactly as specified — nothing else used)

| Layer          | Technology                                             |
|----------------|---------------------------------------------------------|
| Frontend       | HTML5, CSS3, Bootstrap 5, vanilla JavaScript             |
| Backend        | ASP.NET Core 9 Razor Pages (C#)                          |
| ORM            | Entity Framework Core 9 (Code First)                     |
| Database       | Microsoft SQL Server                                     |
| Auth           | ASP.NET Identity + Role-Based Access Control (RBAC)       |
| Architecture   | Three-Tier (Presentation / Application / Data)            |

## Prerequisites

1. **.NET 9 SDK** — https://dotnet.microsoft.com/download
2. **SQL Server** (LocalDB, Express, or full SQL Server). LocalDB ships with
   Visual Studio; on VS Code / other OS install SQL Server or adjust the
   connection string to point to any reachable instance.
3. **Visual Studio 2022 (17.8+)** or **VS Code** with the C# Dev Kit extension.

## Setup Instructions

```bash
# 1. Restore NuGet packages
cd LabourSkillsPlatform
dotnet restore

# 2. Install the EF Core CLI tool if you don't have it
dotnet tool install --global dotnet-ef

# 3. Create the initial migration (generates the SQL schema from the Models)
dotnet ef migrations add InitialCreate

# 4. Apply the migration to create the database
dotnet ef database update

# 5. Run the application
dotnet run
```

The app will seed itself on first run (`Data/DbInitializer.cs`):
- Creates the three RBAC roles: `Admin`, `Employer`, `Labourer`
- Creates a default Admin account:
  - **Email:** `admin@lsp.local`
  - **Password:** `Admin@12345`
  - ⚠️ Change this password immediately via Admin > System Settings after first login.
- Seeds baseline Categories and Skills (Electrical, Plumbing, Masonry & Construction,
  Carpentry, Painting, Cleaning Services) so the system is demo-ready.

Open `https://localhost:<port>` in your browser once `dotnet run` starts.

## Connection String

Edit `appsettings.json` / `appsettings.Development.json` if your SQL Server
instance is not the default LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LabourSkillsPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

## How the thesis maps to the code

| Thesis Feature (Ch. 1 & Ch. 3)         | Where it lives                                                        |
|------------------------------------------|------------------------------------------------------------------------|
| RBAC (Admin/Employer/Labourer)            | `Program.cs` (AuthorizeAreaFolder), `Models/Roles.cs`                    |
| Three-Tier Architecture                   | Pages/Areas (Presentation), Services (Application), Data (Data Tier)     |
| Skill verification (Ch. 2, Research Gap 1)| `Models/LabourerSkill.cs`, `Areas/Admin/Pages/Skills`, `Areas/Labourer/Pages/Skills` |
| Context-aware matching (Ch. 2, Gap 2)     | `Services/JobMatchingService.cs`                                        |
| Employer approval / trust gate            | `Models/Employer.IsApproved`, `Areas/Admin/Pages/Employers`              |
| Reviews & reputation                      | `Models/Review.cs`, `Services/ReviewService.cs`                          |

## Default Test Accounts (create via Register page)

- Register a new account and choose **Labourer** or **Employer** on the sign-up form.
- Employers must be approved by the seeded Admin account before they can post jobs
  (Admin > Approve Employers).
- Labourer skills must be verified by the Admin before they count fully in the
  matching score (Admin > Verify Skills).

## Notes

- This code was authored outside of a live .NET build environment. Before your
  defense, run a full `dotnet build` locally and fix any environment-specific
  NuGet version mismatches (e.g. if only .NET 8 SDK is available, retarget
  `TargetFramework` in the `.csproj` to `net8.0` and adjust package versions
  to their 8.x equivalents).
- No feature outside your three thesis chapters was added (no payments, no chat,
  no external APIs) to keep the system consistent with your defended scope.
