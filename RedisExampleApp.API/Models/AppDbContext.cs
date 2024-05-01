using Microsoft.EntityFrameworkCore;

namespace RedisExampleApp.API.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasData(
                new Product()
                {
                    Id = Guid.NewGuid(),
                    Price = 12,
                    Name = "Kalem 1"
                },
                new Product()
                {
                    Id = Guid.NewGuid(),
                    Price = 14,
                    Name = "Kalem 2"
                }, 
                new Product()
                {
                    Id = Guid.NewGuid(),
                    Price = 15,
                    Name = "Kalem 2"
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
