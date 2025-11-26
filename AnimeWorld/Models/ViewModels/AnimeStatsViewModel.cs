public class AnimeStatsViewModel
{
    public int TotalCharacters { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvgBalance { get; set; }
    public int AliveCount { get; set; }
    public int DeadCount { get; set; }
    public int MaxEpisodes { get; set; }
    public int MinEpisodes { get; set; }

    public List<GenreCountVM> CharactersByGenre { get; set; } = new List<GenreCountVM>();
}

public class GenreCountVM
{
    public string Genre { get; set; }
    public int TotalCharacters { get; set; }
}
