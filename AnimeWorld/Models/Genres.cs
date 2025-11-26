namespace AnimeWorld.Models
{
    public class Genres
    {
        public int GenresId { get; set; }
        public string GenresName { get; set; }
        public ICollection<AnimeCharacter> AnimeCharacters { get; set; } = new List<AnimeCharacter>();
    }
}
