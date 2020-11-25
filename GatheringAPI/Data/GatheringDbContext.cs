using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace GatheringAPI.Data
{
    public class GatheringDbContext : IdentityDbContext<User, IdentityRole<long>, long>
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
            modelBuilder.Entity<EventInvite>()
                .HasKey(eventInvite => new
                {
                    eventInvite.UserId,
                    eventInvite.EventId
                });
            modelBuilder.Entity<GroupUser>()
                .HasKey(groupUser => new
                {
                    groupUser.GroupId,
                    groupUser.UserId
                });
            modelBuilder.Entity<HostedEvent>()
                .HasKey(hostedEvent => new
                {
                    hostedEvent.EventId,
                    hostedEvent.UserId
                });
            modelBuilder.Entity<EventComment>()
                .HasKey(eventComment => new
                {
                    eventComment.EventId,
                    eventComment.UserId
                });
            modelBuilder.Entity<JoinRequest>()
                .HasKey(joinRequest => new
                {
                    joinRequest.GroupId,
                    joinRequest.UserId
                });
        }
        public DbSet<Group> Groups { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<GroupEvent> GroupEvents { get; set; }

        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<EventInvite> EventInvites { get; set; }
        public DbSet<HostedEvent> HostedEvents { get; set; }
        public DbSet<EventComment> EventComments { get; set; }
        public DbSet<JoinRequest> JoinRequests { get; set; }
    }
}
