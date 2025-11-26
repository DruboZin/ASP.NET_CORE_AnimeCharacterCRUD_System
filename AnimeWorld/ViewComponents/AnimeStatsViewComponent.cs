using AnimeWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AnimeStatsViewComponent : ViewComponent
{
    private readonly AnimeCharacterDbContext _context;

    public AnimeStatsViewComponent(AnimeCharacterDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var model = new AnimeStatsViewModel
        {
            TotalCharacters = _context.AnimeCharacters.Count(),
            TotalBalance = _context.AnimeCharacters.Sum(c => c.BankBalance),
            AvgBalance = _context.AnimeCharacters.Average(c => c.BankBalance),
            AliveCount = _context.AnimeCharacters.Count(c => c.IsAlive),
            DeadCount = _context.AnimeCharacters.Count(c => !c.IsAlive),

            MaxEpisodes = _context.AnimeNames.Any() ? _context.AnimeNames.Max(a => a.TotalEp) : 0,
            MinEpisodes = _context.AnimeNames.Any() ? _context.AnimeNames.Min(a => a.TotalEp) : 0,

            CharactersByGenre = _context.AnimeCharacters
                .GroupBy(c => c.Genres.GenresName)
                .Select(g => new GenreCountVM
                {
                    Genre = g.Key,
                    TotalCharacters = g.Count()
                })
                .ToList()
        };

        return View(model);
    }
}
