using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnimeWorld.Models;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Authorization;


namespace AnimeWorld.Controllers
{
    [Authorize]
    public class AnimeCharactersController : Controller
    {

        private readonly AnimeCharacterDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AnimeCharactersController> _logger;

        public AnimeCharactersController(AnimeCharacterDbContext db, IWebHostEnvironment env, ILogger<AnimeCharactersController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        // GET: AnimeCharacter
        // GET: AnimeCharacter
        //public async Task<IActionResult> Index(string searchString)
        //{
        //    ViewData["CurrentFilter"] = searchString;

        //    var characters = _db.AnimeCharacters
        //                        .Include(c => c.Genres)
        //                        .Include(c => c.AnimeNames)
        //                        .AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(searchString))
        //    {
        //        characters = characters.Where(c => c.AnimeCharacterName.Contains(searchString));
        //    }

        //    return View(await characters.ToListAsync());
        //}
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


        // GET: AnimeCharacter/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var character = await _db.AnimeCharacters
        //        .Include(c => c.Genres)
        //        .Include(c => c.AnimeNames)
        //        .FirstOrDefaultAsync(c => c.AnimeCharacterId == id);

        //    if (character == null) return NotFound();

        //    return View(character);
        //}

        // GET: AnimeCharacter/Create
        public async Task<IActionResult> Create()
        {
            await PopulateGenresDropDown();
            return View(new AnimeCharacterVM
            {
                DateOfBirth = DateTime.Today,
                AnimeNames = new List<AnimeName>() // empty by default
            });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnimeCharacterVM vm)
        {
            vm.AnimeNames = vm.AnimeNames?.Where(a => !string.IsNullOrWhiteSpace(a.AnimationName)).ToList() ?? new List<AnimeName>();

            //if (!ModelState.IsValid)
            //{
            //    LogModelState();
            //    await PopulateGenresDropDown(vm.GenresId);
            //    return View(vm);
            //}

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

            var animeCharacter = new AnimeCharacter
            {
                AnimeCharacterName = vm.AnimeCharacterName,
                DateOfBirth = vm.DateOfBirth,
                BankBalance = vm.BankBalance,
                IsAlive = vm.IsAlive,
                GenresId = vm.GenresId,
                Address = vm.Address
            };

            foreach (var anime in vm.AnimeNames)
            {
                animeCharacter.AnimeNames.Add(new AnimeName
                {
                    AnimationName = anime.AnimationName,
                    TotalEp = anime.TotalEp,
                    OnGoing = anime.OnGoing
                });
            }

            if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
            {
                animeCharacter.CharacterPicture = await SavePictureFile(vm.CharacterPictureFile);
            }

            _db.AnimeCharacters.Add(animeCharacter);
            await _db.SaveChangesAsync();

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
        // GET: AnimeCharacters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var character = await _db.AnimeCharacters
                .Include(c => c.AnimeNames)
                .FirstOrDefaultAsync(c => c.AnimeCharacterId == id);

            if (character == null)
                return NotFound();

            var vm = new AnimeCharacterVM
            {
                AnimeCharacterId = character.AnimeCharacterId,
                AnimeCharacterName = character.AnimeCharacterName,
                DateOfBirth = character.DateOfBirth,
                BankBalance = character.BankBalance,
                IsAlive = character.IsAlive,
                GenresId = character.GenresId,
                CharacterPicture = character.CharacterPicture,
                AnimeNames = character.AnimeNames?.Select(a => new AnimeName
                {
                    AnimeNameId = a.AnimeNameId,
                    AnimationName = a.AnimationName,
                    TotalEp = a.TotalEp,
                    OnGoing = a.OnGoing
                }).ToList() ?? new List<AnimeName>()
            };

            await PopulateGenresDropDown(vm.GenresId);
            return View(vm);
        }

        // POST: AnimeCharacters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnimeCharacterVM vm)
        {
            if (id != vm.AnimeCharacterId)
                return NotFound();

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

            var character = await _db.AnimeCharacters
                .Include(c => c.AnimeNames)
                .FirstOrDefaultAsync(c => c.AnimeCharacterId == id);

            if (character == null)
                return NotFound();

            // Update main properties
            character.AnimeCharacterName = vm.AnimeCharacterName;
            character.DateOfBirth = vm.DateOfBirth;
            character.BankBalance = vm.BankBalance;
            character.IsAlive = vm.IsAlive;
            character.GenresId = vm.GenresId;
            character.Address = vm.Address;

            // Update image if new one is uploaded
            if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(character.CharacterPicture))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "uploads", character.CharacterPicture);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                character.CharacterPicture = await SavePictureFile(vm.CharacterPictureFile);
            }

            // Update Anime List
            _db.AnimeNames.RemoveRange(character.AnimeNames); // clear old ones
            foreach (var anime in vm.AnimeNames)
            {
                character.AnimeNames.Add(new AnimeName
                {
                    AnimationName = anime.AnimationName,
                    TotalEp = anime.TotalEp,
                    OnGoing = anime.OnGoing
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: AnimeCharacters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var animeCharacter = await _db.AnimeCharacters
                .Include(a => a.Genres)
                .Include(a => a.AnimeNames)
                .FirstOrDefaultAsync(a => a.AnimeCharacterId == id);

            if (animeCharacter == null)
                return NotFound();

            var vm = new AnimeCharacterVM
            {
                AnimeCharacterId = animeCharacter.AnimeCharacterId,
                AnimeCharacterName = animeCharacter.AnimeCharacterName,
                DateOfBirth = animeCharacter.DateOfBirth,
                BankBalance = animeCharacter.BankBalance,
                IsAlive = animeCharacter.IsAlive,
                GenresId = animeCharacter.GenresId,
                CharacterPicture = animeCharacter.CharacterPicture,
                Address = animeCharacter.Address,
                AnimeNames = animeCharacter.AnimeNames?.ToList() ?? new List<AnimeName>()
            };

            ViewBag.GenreName = animeCharacter.Genres?.GenresName ?? "Unknown";
            return View(vm);
        }
        // GET: AnimeCharacters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var animeCharacter = await _db.AnimeCharacters
                .Include(a => a.Genres)
                .Include(a => a.AnimeNames)
                .FirstOrDefaultAsync(a => a.AnimeCharacterId == id);

            if (animeCharacter == null)
                return NotFound();

            var vm = new AnimeCharacterVM
            {
                AnimeCharacterId = animeCharacter.AnimeCharacterId,
                AnimeCharacterName = animeCharacter.AnimeCharacterName,
                DateOfBirth = animeCharacter.DateOfBirth,
                BankBalance = animeCharacter.BankBalance,
                IsAlive = animeCharacter.IsAlive,
                GenresId = animeCharacter.GenresId,
                CharacterPicture = animeCharacter.CharacterPicture,
                AnimeNames = animeCharacter.AnimeNames?.ToList() ?? new List<AnimeName>()
            };

            ViewBag.GenreName = animeCharacter.Genres?.GenresName ?? "Unknown";

            return View(vm);
        }

        // POST: AnimeCharacters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animeCharacter = await _db.AnimeCharacters.FindAsync(id);
            if (animeCharacter != null)
            {
                // Delete image file if exists
                if (!string.IsNullOrEmpty(animeCharacter.CharacterPicture))
                {
                    var filePath = Path.Combine(_env.WebRootPath, "uploads", animeCharacter.CharacterPicture);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _db.AnimeCharacters.Remove(animeCharacter);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        // GET: AnimeCharacter/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var character = await _db.AnimeCharacters.FindAsync(id);
        //    if (character == null) return NotFound();

        //    var vm = new AnimeCharacterVM
        //    {
        //        AnimeCharacterId = character.AnimeCharacterId,
        //        AnimeCharacterName = character.AnimeCharacterName,
        //        DateOfBirth = character.DateOfBirth,
        //        BankBalance = character.BankBalance,
        //        IsAlive = character.IsAlive,
        //        CharacterPicture = character.CharacterPicture,
        //        GenresId = character.GenresId
        //    };

        //    await PopulateGenresDropDown(vm.GenresId);
        //    return View(vm);
        //}

        //// POST: AnimeCharacter/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, AnimeCharacterVM vm)
        //{
        //    if (id != vm.AnimeCharacterId) return NotFound();

        //    if (!ModelState.IsValid)
        //    {
        //        LogModelState();
        //        await PopulateGenresDropDown(vm.GenresId);
        //        return View(vm);
        //    }

        //    var character = await _db.AnimeCharacters.FindAsync(id);
        //    if (character == null) return NotFound();

        //    character.AnimeCharacterName = vm.AnimeCharacterName;
        //    character.DateOfBirth = vm.DateOfBirth;
        //    character.BankBalance = vm.BankBalance;
        //    character.IsAlive = vm.IsAlive;
        //    character.GenresId = vm.GenresId;

        //    if (vm.CharacterPictureFile != null && vm.CharacterPictureFile.Length > 0)
        //    {
        //        // Delete old picture if exists
        //        if (!string.IsNullOrEmpty(character.CharacterPicture))
        //        {
        //            var oldPath = Path.Combine(_env.WebRootPath, "uploads", character.CharacterPicture);
        //            if (System.IO.File.Exists(oldPath))
        //                System.IO.File.Delete(oldPath);
        //        }
        //        character.CharacterPicture = await SavePictureFile(vm.CharacterPictureFile);
        //    }

        //    _db.Update(character);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}

        // GET: AnimeCharacter/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var character = await _db.AnimeCharacters
        //        .Include(c => c.Genres)
        //        .FirstOrDefaultAsync(c => c.AnimeCharacterId == id);

        //    if (character == null) return NotFound();

        //    return View(character);
        //}

        //// POST: AnimeCharacter/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var character = await _db.AnimeCharacters.FindAsync(id);
        //    if (character == null) return NotFound();

        //    if (!string.IsNullOrEmpty(character.CharacterPicture))
        //    {
        //        var filePath = Path.Combine(_env.WebRootPath, "uploads", character.CharacterPicture);
        //        if (System.IO.File.Exists(filePath))
        //            System.IO.File.Delete(filePath);
        //    }

        //    _db.AnimeCharacters.Remove(character);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}








    }
}
