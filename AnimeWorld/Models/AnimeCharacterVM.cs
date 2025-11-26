using System.ComponentModel.DataAnnotations;

namespace AnimeWorld.Models
{
    public class AnimeCharacterVM
    {
        public int AnimeCharacterId { get; set; }
        [Required, StringLength(50)]
        public string AnimeCharacterName { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public decimal BankBalance { get; set; }
        public bool IsAlive { get; set; }
        public string CharacterPicture { get; set; }
        public IFormFile? CharacterPictureFile { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }  // <-- new property

        [Required]
        public int GenresId { get; set; }
        public ICollection<AnimeName> AnimeNames { get; set; } = new List<AnimeName>();
    }
}
