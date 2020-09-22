using GatheringAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

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
            modelBuilder.Entity<User>()
                .HasData(
                new User { UserId = 1, FirstName = "Bob", LastName = "Bobberton", Email = "Bobby@example.com", BirthDate = new DateTime(1990,1,1) }
                );

        }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
    }
}
