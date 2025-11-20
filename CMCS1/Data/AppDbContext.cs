using CMCS1.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data for testing
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "lecturer", Password = "password", Role = "Lecturer", FullName = "John Doe", Email = "john@example.com", Phone = "123-456-7890", HourlyRate = 50.00m },
                new User { UserId = 2, Username = "manager1", Password = "password", Role = "Manager1", FullName = "Jane Smith", Email = "jane@example.com", Phone = "123-456-7891" },
                new User { UserId = 3, Username = "manager2", Password = "password", Role = "Manager2", FullName = "Bob Jones", Email = "bob@example.com", Phone = "123-456-7892" },
                new User { UserId = 4, Username = "hr", Password = "password", Role = "HR", FullName = "Alice Brown", Email = "alice@example.com", Phone = "123-456-7893" }
            );
        }
    }
}
