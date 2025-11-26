using System.ComponentModel.DataAnnotations;

namespace AnimeWorld.Models
{
    public class AnimeNameVM
    {
        public int AnimeNameId { get; set; }
        [Required, StringLength(50)]
        public string AnimationName { get; set; }
        public int TotalEp { get; set; }
        public bool OnGoing { get; set; }
    }
}
