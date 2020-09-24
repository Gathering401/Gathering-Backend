using GatheringAPI.Data;
using GatheringAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GatheringAPI.Test
{
    public abstract class TestBase : IDisposable
    {
        private readonly SqliteConnection _connection;
        protected readonly GatheringDbContext _db;

        public TestBase()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _db = new GatheringDbContext(
                new DbContextOptionsBuilder<GatheringDbContext>()
                .UseSqlite(_connection)
                .Options);
            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _db?.Dispose();
            _connection?.Dispose();
        }
       //protected async Task<Group> CreateAndSaveGroup()
        //{
           // var group = new Group { };
        //}
    }
}
