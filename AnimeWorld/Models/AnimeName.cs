using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeWorld.Models
{
    public class AnimeName
    {
        public int AnimeNameId { get; set; }
        [Required, StringLength(50)]
        public string AnimationName { get; set; }
        public int TotalEp { get; set; }
        public bool OnGoing { get; set; }

        [ForeignKey("AnimeCharacter")]
        public int AnimeCharacterId { get; set; }
        public AnimeCharacter AnimeCharacter { get; set; }
    }
}
