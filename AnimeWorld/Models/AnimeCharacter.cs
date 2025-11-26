using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeWorld.Models
{
    public class AnimeCharacter
    {
        public int AnimeCharacterId { get; set; }

        [Required, StringLength(50)]
        public string AnimeCharacterName { get; set; }

        [Required, Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal BankBalance { get; set; }

        public bool IsAlive { get; set; }

        public string CharacterPicture { get; set; }
       //public IFormFile CharacterPicture { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }

        [ForeignKey("Genres")]
        public int GenresId { get; set; }
        public Genres Genres { get; set; }

        public ICollection<AnimeName> AnimeNames { get; set; } = new List<AnimeName>();

        // Auto-calculated properties
        [NotMapped]
        public int Age => (int)((DateTime.Now - DateOfBirth).TotalDays / 365.25);

        [NotMapped]
        public int TotalAnimeEpisodes => AnimeNames?.Sum(a => a.TotalEp) ?? 0;

        [NotMapped]
        public string Status => IsAlive ? "Alive" : "Dead";
    }

}
