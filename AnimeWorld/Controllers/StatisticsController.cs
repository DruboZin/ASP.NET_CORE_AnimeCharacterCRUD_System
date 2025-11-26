using AnimeWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StatisticsController : Controller
{
    private readonly AnimeCharacterDbContext _context;

    public StatisticsController(AnimeCharacterDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Basic aggregates
        var totalCharacters = _context.AnimeCharacters.Count();
        var totalBalance = _context.AnimeCharacters.Sum(c => c.BankBalance);
        var averageBalance = _context.AnimeCharacters.Average(c => c.BankBalance);

        // Alive / Dead count
        var aliveCount = _context.AnimeCharacters.Count(c => c.IsAlive);
        var deadCount = _context.AnimeCharacters.Count(c => !c.IsAlive);

        // Max/Min episodes
        var maxEpisodes = _context.AnimeNames.Any() ? _context.AnimeNames.Max(a => a.TotalEp) : 0;
        var minEpisodes = _context.AnimeNames.Any() ? _context.AnimeNames.Min(a => a.TotalEp) : 0;

        // Total episodes per character
        var episodeTotals = _context.AnimeCharacters
            .Select(c => new
            {
                c.AnimeCharacterName,
                TotalEpisodes = c.AnimeNames.Sum(a => a.TotalEp)
            })
            .ToList();

        // Total characters per genre
        var charactersByGenre = _context.AnimeCharacters
            .GroupBy(c => c.Genres.GenresName)
            .Select(g => new
            {
                Genre = g.Key,
                TotalCharacters = g.Count()
            })
            .ToList();

        // Send to View using ViewBag
        ViewBag.TotalCharacters = totalCharacters;
        ViewBag.TotalBalance = totalBalance;
        ViewBag.AverageBalance = averageBalance;
        ViewBag.AliveCount = aliveCount;
        ViewBag.DeadCount = deadCount;
        ViewBag.MaxEpisodes = maxEpisodes;
        ViewBag.MinEpisodes = minEpisodes;
        ViewBag.EpisodeTotals = episodeTotals;
        ViewBag.CharactersByGenre = charactersByGenre;

        return View();
    }
}
