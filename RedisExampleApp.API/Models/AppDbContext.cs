using Microsoft.EntityFrameworkCore;

namespace RedisExampleApp.API.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var Id = Guid.NewGuid();

            modelBuilder.Entity<Product>()
                .HasData(
                new Product()
                {
                    Id = Id,
                    Name = "Kalem 1"
                },
                new Product()
                {
                    Id = Id,
                    Name = "Kalem 2"
                }, 
                new Product()
                {
                    Id = Id,
                    Name = "Kalem 2"
                });
            base.OnModelCreating(modelBuilder);
        }
    }
}
