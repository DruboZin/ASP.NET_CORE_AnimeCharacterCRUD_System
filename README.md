# 🌟 AnimeWorld — ASP.NET Core MVC Master-Details Application

**AnimeWorld** is a complete ASP.NET Core MVC application designed to manage Anime Characters, their genres, and their anime titles. The project demonstrates advanced features like **Identity Authentication**, **Stored Procedure integration**, **Master-Details CRUD**, **Image Upload**, and **Pagination**.

---

## 📌 Features

### ✅ Authentication & Authorization
- Integrated with **ASP.NET Core Identity**
- Role-based authorization (`Admin`) for editing and deleting characters
- `[Authorize]` attributes protect routes
- `[AllowAnonymous]` for public access pages (Index, Details)

### ✅ CRUD Operations
- **Create, Read, Update, Delete** for Anime Characters
- **Master-Details**: One Anime Character → Multiple Anime Names
- Edit replaces old AnimeNames completely
- Delete removes associated AnimeNames and uploaded images

### ✅ Image Upload
- Supports `.jpg`, `.jpeg`, `.png`, `.gif`
- Maximum file size: 2 MB
- Old images are deleted on update
- Stored in `wwwroot/uploads`

### ✅ Stored Procedure Integration
- `sp_InsertAnimeCharacter` — Add new characters
- `sp_InsertAnimeName` — Add anime titles per character
- `sp_UpdateAnimeCharacter` — Update characters
- `sp_DeleteAnimeCharacterById` — Delete character & related anime
- `sp_DeleteAnimeNamesByCharacter` — Delete all anime of a character
- `sp_GetAnimeCharacterById` — Fetch details for view pages

### ✅ Pagination & Search
- Search by character name
- Pagination with **X.PagedList** (`pageSize = 2`)
- Sorted alphabetically

### ✅ View Components & Stats
- `AnimeStatsViewComponent` displays:
  - Total Characters
  - Total and Average BankBalance
  - Alive vs Dead counts
  - Max & Min Anime Episodes
  - Characters by Genre

---

## 🧩 Technologies Used

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core MVC | Web framework |
| Entity Framework Core | ORM for database access |
| SQL Server | Database |
| ASP.NET Core Identity | Authentication and Authorization |
| Stored Procedures | Efficient database operations |
| X.PagedList | Pagination support |
| Bootstrap 5 | UI styling |
| IFormFile | Image upload |

---

## 📂 Project Structure
AnimeWorld/
│
├── Controllers/
│ └── AnimeGokuController.cs
│
├── Models/
│ ├── AnimeCharacter.cs
│ ├── AnimeName.cs
│ ├── Genres.cs
│ ├── ApplicationUser.cs
│ ├── ViewModels/
│ ├── AnimeCharacterVM.cs
│ ├── AnimeNameVM.cs
│ ├── LoginVM.cs
│ ├── RegisterVM.cs
│ ├── AnimeStatsViewModel.cs
│ └── GenreCountVM.cs
│
├── Data/
│ └── AnimeCharacterDbContext.cs
│
├── Views/
│ ├── AnimeGoku/
│ │ ├── Index.cshtml
│ │ ├── Create.cshtml
│ │ ├── Edit.cshtml
│ │ ├── Delete.cshtml
│ │ └── Details.cshtml
│ └── Shared/
│ └── Components/
│ └── AnimeStats/
│ └── Default.cshtml
│
├── wwwroot/uploads/ ← Uploaded images
├── appsettings.json
└── Migrations/

---

## 🛠 Database Models

### AnimeCharacter
- `AnimeCharacterId`, `AnimeCharacterName`, `DateOfBirth`, `BankBalance`, `IsAlive`, `CharacterPicture`, `Address`, `GenresId`
- Navigation: `Genres`, `AnimeNames`
- Calculated properties: `Age`, `TotalAnimeEpisodes`, `Status`

### AnimeName
- `AnimeNameId`, `AnimationName`, `TotalEp`, `OnGoing`
- Foreign key: `AnimeCharacterId`
- Navigation: `AnimeCharacter`

### Genres
- `GenresId`, `GenresName`
- Navigation: `AnimeCharacters`

### Identity
- `ApplicationUser` inherits from `IdentityUser`

---

## 🔧 How to Run

### 1️⃣ Clone the repository
```bash
git clone https://github.com/YOUR-USERNAME/AnimeWorld.git
cd AnimeWorld
🖼 Image Upload Rules

Allowed extensions: .jpg, .jpeg, .png, .gif

Max file size: 2 MB

Saved as GUID in wwwroot/uploads

Old images are deleted on update or delete
📊 Stats View Component

AnimeStatsViewComponent aggregates:

Total characters

Bank balances

Alive/Dead count

Max/Min episodes

Characters grouped by genre

