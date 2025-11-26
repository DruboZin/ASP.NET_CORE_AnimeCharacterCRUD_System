using AnimeWorld.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace AnimeWorld.Controllers
{
    [Authorize]
    public class AnimeGokuController : Controller
    {
        private readonly AnimeCharacterDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration config;
        private readonly ILogger<AnimeGokuController> _logger;

        public AnimeGokuController(AnimeCharacterDbContext db, IWebHostEnvironment env, IConfiguration config, ILogger<AnimeGokuController> logger)
        {
            _db = db;
            _env = env;
            this.config = config;
            _logger = logger;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;

            var characters = _db.AnimeCharacters
                                .Include(c => c.Genres)
                                .Include(c => c.AnimeNames)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                characters = characters.Where(c => c.AnimeCharacterName.Contains(searchString));
            }

            int pageSize = 2;
            int pageNumber = page ?? 1;

            var pagedList = characters
                  .OrderBy(c => c.AnimeCharacterName)
                  .ToPagedList(pageNumber, pageSize);


            return View(pagedList);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            await PopulateGenresDropDown();
            return View(new AnimeCharacterVM
            {
                DateOfBirth = DateTime.Today,
                AnimeNames = new List<AnimeName>() // empty by default
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(AnimeCharacterVM vm)
        {
            vm.AnimeNames = vm.AnimeNames?.Where(a => !string.IsNullOrWhiteSpace(a.AnimationName)).ToList() ?? new List<AnimeName>();

            if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
            {
                var (ok, err) = ValidatePictureFile(vm.CharacterPictureFile);
                if (!ok)
                {
                    ModelState.AddModelError(nameof(vm.CharacterPictureFile), err);
                    await PopulateGenresDropDown(vm.GenresId);
                    return View(vm);
                }
            }

            string pictureFileName = null;
            if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
            {
                pictureFileName = await SavePictureFile(vm.CharacterPictureFile);
            }

            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_InsertAnimeCharacter", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@AnimeCharacterName", vm.AnimeCharacterName);
            cmd.Parameters.AddWithValue("@DateOfBirth", vm.DateOfBirth);
            cmd.Parameters.AddWithValue("@BankBalance", vm.BankBalance);
            cmd.Parameters.AddWithValue("@IsAlive", vm.IsAlive);
            cmd.Parameters.AddWithValue("@CharacterPicture", pictureFileName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Address", vm.Address);
            cmd.Parameters.AddWithValue("@GenresId", vm.GenresId);

            var outputId = new SqlParameter("@NewAnimeCharacterId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outputId);

            await cmd.ExecuteNonQueryAsync();

            int newCharacterId = (int)outputId.Value;

            // Insert AnimeNames
            foreach (var anime in vm.AnimeNames)
            {
                using var cmdAnime = new SqlCommand("sp_InsertAnimeName", conn);
                cmdAnime.CommandType = CommandType.StoredProcedure;
                cmdAnime.Parameters.AddWithValue("@AnimationName", anime.AnimationName);
                cmdAnime.Parameters.AddWithValue("@TotalEp", anime.TotalEp);
                cmdAnime.Parameters.AddWithValue("@OnGoing", anime.OnGoing);
                cmdAnime.Parameters.AddWithValue("@AnimeCharacterId", newCharacterId);
                await cmdAnime.ExecuteNonQueryAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateGenresDropDown(object selected = null)
        {
            var genres = await _db.Genress.OrderBy(g => g.GenresName).ToListAsync();
            ViewBag.Genres = new SelectList(genres, "GenresId", "GenresName", selected);
        }

        private (bool ok, string error) ValidatePictureFile(IFormFile file)
        {
            if (file == null) return (true, null);

            var permitted = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !permitted.Contains(ext))
            {
                return (false, "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
            }

            const long maxBytes = 2 * 1024 * 1024;
            if (file.Length > maxBytes)
            {
                return (false, "Maximum file size is 2 MB.");
            }

            return (true, null);
        }

        private async Task<string> SavePictureFile(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using var fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            return fileName;
        }

        private void LogModelState()
        {
            var errors = ModelState.Where(x => x.Value.Errors.Any())
                .Select(x => new
                {
                    Key = x.Key,
                    Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                }).ToList();

            if (errors.Any())
            {
                _logger.LogWarning("ModelState errors: {@Errors}", errors);
                foreach (var e in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"{e.Key}: {string.Join(", ", e.Errors)}");
                }
            }
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            AnimeCharacterVM vm;

            using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                // Fetch AnimeCharacter
                using var cmd = new SqlCommand("SELECT * FROM AnimeCharacters WHERE AnimeCharacterId = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id.Value);
                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows) return NotFound();

                await reader.ReadAsync();
                vm = new AnimeCharacterVM
                {
                    AnimeCharacterId = (int)reader["AnimeCharacterId"],
                    AnimeCharacterName = reader["AnimeCharacterName"].ToString(),
                    DateOfBirth = (DateTime)reader["DateOfBirth"],
                    BankBalance = (decimal)reader["BankBalance"],
                    IsAlive = (bool)reader["IsAlive"],
                    CharacterPicture = reader["CharacterPicture"].ToString(),
                    Address = reader["Address"].ToString(),
                    GenresId = (int)reader["GenresId"],
                    AnimeNames = new List<AnimeName>()
                };
                reader.Close();

                // Fetch AnimeNames
                using var cmdAnime = new SqlCommand("SELECT * FROM AnimeNames WHERE AnimeCharacterId = @Id", conn);
                cmdAnime.Parameters.AddWithValue("@Id", id.Value);
                using var readerAnime = await cmdAnime.ExecuteReaderAsync();
                while (await readerAnime.ReadAsync())
                {
                    vm.AnimeNames.Add(new AnimeName
                    {
                        AnimeNameId = (int)readerAnime["AnimeNameId"],
                        AnimationName = readerAnime["AnimationName"].ToString(),
                        TotalEp = (int)readerAnime["TotalEp"],
                        OnGoing = (bool)readerAnime["OnGoing"]
                    });
                }
            }

            await PopulateGenresDropDown(vm.GenresId);
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, AnimeCharacterVM vm)
        {
            if (id != vm.AnimeCharacterId) return NotFound();

            // Remove empty anime names
            vm.AnimeNames = vm.AnimeNames?.Where(a => !string.IsNullOrWhiteSpace(a.AnimationName)).ToList() ?? new List<AnimeName>();

            string pictureFileName = vm.CharacterPicture; // keep old picture by default

            // Only update picture if a new file is uploaded
            if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
            {
                var (ok, err) = ValidatePictureFile(vm.CharacterPictureFile);
                if (!ok)
                {
                    ModelState.AddModelError(nameof(vm.CharacterPictureFile), err);
                    await PopulateGenresDropDown(vm.GenresId);
                    return View(vm);
                }

                // Delete old picture if exists
                if (!string.IsNullOrEmpty(pictureFileName))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "uploads", pictureFileName);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Save new picture
                pictureFileName = await SavePictureFile(vm.CharacterPictureFile);
            }

            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            // Update main AnimeCharacter
            using var cmd = new SqlCommand("sp_UpdateAnimeCharacter", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AnimeCharacterId", vm.AnimeCharacterId);
            cmd.Parameters.AddWithValue("@AnimeCharacterName", vm.AnimeCharacterName);
            cmd.Parameters.AddWithValue("@DateOfBirth", vm.DateOfBirth);
            cmd.Parameters.AddWithValue("@BankBalance", vm.BankBalance);
            cmd.Parameters.AddWithValue("@IsAlive", vm.IsAlive);
            cmd.Parameters.AddWithValue("@CharacterPicture", pictureFileName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Address", vm.Address);
            cmd.Parameters.AddWithValue("@GenresId", vm.GenresId);
            await cmd.ExecuteNonQueryAsync();

            // Delete old AnimeNames
            using var cmdDelete = new SqlCommand("sp_DeleteAnimeNamesByCharacter", conn);
            cmdDelete.CommandType = CommandType.StoredProcedure;
            cmdDelete.Parameters.AddWithValue("@AnimeCharacterId", vm.AnimeCharacterId);
            await cmdDelete.ExecuteNonQueryAsync();

            // Insert updated AnimeNames
            foreach (var anime in vm.AnimeNames)
            {
                using var cmdAnime = new SqlCommand("sp_InsertAnimeName", conn);
                cmdAnime.CommandType = CommandType.StoredProcedure;
                cmdAnime.Parameters.AddWithValue("@AnimationName", anime.AnimationName);
                cmdAnime.Parameters.AddWithValue("@TotalEp", anime.TotalEp);
                cmdAnime.Parameters.AddWithValue("@OnGoing", anime.OnGoing);
                cmdAnime.Parameters.AddWithValue("@AnimeCharacterId", vm.AnimeCharacterId);
                await cmdAnime.ExecuteNonQueryAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            AnimeCharacterVM vm = null;
            string genreName = "Unknown";

            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_GetAnimeCharacterById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AnimeCharacterId", id.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            // Read main character
            if (await reader.ReadAsync())
            {
                vm = new AnimeCharacterVM
                {
                    AnimeCharacterId = (int)reader["AnimeCharacterId"],
                    AnimeCharacterName = reader["AnimeCharacterName"].ToString(),
                    DateOfBirth = reader["DateOfBirth"] as DateTime? ?? DateTime.Today,
                    BankBalance = reader["BankBalance"] as decimal? ?? 0,
                    IsAlive = reader["IsAlive"] as bool? ?? true,
                    GenresId = reader["GenresId"] as int? ?? 0,
                    CharacterPicture = reader["CharacterPicture"]?.ToString(),
                    Address = reader["Address"]?.ToString() ?? "Unknown",
                    AnimeNames = new List<AnimeName>()
                };
                genreName = reader["GenresName"]?.ToString() ?? "Unknown";
            }
            else
            {
                return NotFound();
            }

            // Move to next result for AnimeNames
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.AnimeNames.Add(new AnimeName
                    {
                        AnimeNameId = (int)reader["AnimeNameId"],
                        AnimationName = reader["AnimationName"]?.ToString(),
                        TotalEp = reader["TotalEp"] as int? ?? 0,
                        OnGoing = reader["OnGoing"] as bool? ?? false
                    });
                }
            }

            ViewBag.GenreName = genreName;
            return View(vm);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            AnimeCharacterVM vm = null;
            string genreName = "Unknown";

            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_GetAnimeCharacterById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AnimeCharacterId", id.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vm = new AnimeCharacterVM
                {
                    AnimeCharacterId = (int)reader["AnimeCharacterId"],
                    AnimeCharacterName = reader["AnimeCharacterName"].ToString(),
                    DateOfBirth = reader["DateOfBirth"] as DateTime? ?? DateTime.Today,
                    BankBalance = reader["BankBalance"] as decimal? ?? 0,
                    IsAlive = reader["IsAlive"] as bool? ?? true,
                    GenresId = reader["GenresId"] as int? ?? 0,
                    CharacterPicture = reader["CharacterPicture"]?.ToString(),
                    Address = reader["Address"]?.ToString() ?? "Unknown",
                    AnimeNames = new List<AnimeName>()
                };
                genreName = reader["GenresName"]?.ToString() ?? "Unknown";
            }
            else
            {
                return NotFound();
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    vm.AnimeNames.Add(new AnimeName
                    {
                        AnimeNameId = (int)reader["AnimeNameId"],
                        AnimationName = reader["AnimationName"]?.ToString(),
                        TotalEp = reader["TotalEp"] as int? ?? 0,
                        OnGoing = reader["OnGoing"] as bool? ?? false
                    });
                }
            }

            ViewBag.GenreName = genreName;
            return View(vm);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_DeleteAnimeCharacterById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AnimeCharacterId", id);

            await cmd.ExecuteNonQueryAsync();

            return RedirectToAction(nameof(Index));
        }







    }
}
