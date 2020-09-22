using GatheringAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GatheringAPI.Data
{
    public class GatheringDbContext : DbContext
    {
        public GatheringDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
    }
}
