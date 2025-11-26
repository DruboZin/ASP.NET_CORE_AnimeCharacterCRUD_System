using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AnimeWorld.Models
{
    public class AnimeCharacterDbContext : IdentityDbContext<ApplicationUser>
    {
        public AnimeCharacterDbContext(DbContextOptions<AnimeCharacterDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnimeCharacter> AnimeCharacters { get; set; }
        public DbSet<Genres> Genress { get; set; }
        public DbSet<AnimeName> AnimeNames { get; set; }
    }
}
