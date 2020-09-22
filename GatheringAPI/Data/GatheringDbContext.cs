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

            modelBuilder.Entity<Group>()
                .HasData(
                    new Group { GroupId = 1, GroupName = "Odysseus", Description = "HI", Location = "Remote" }
                );
            modelBuilder.Entity<GroupEvent>()
                .HasKey(groupEvent => new
                {
                    groupEvent.GroupId,
                    groupEvent.EventId,
                });
        }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<GroupEvent> GroupEvents { get; set; }
    }
}
