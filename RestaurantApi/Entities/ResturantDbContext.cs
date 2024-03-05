using Microsoft.EntityFrameworkCore;

namespace RestaurantAPI.Entities
{
    public class ResturantDbContext : DbContext
    {
        private string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=ResturantDb;Trusted_Connection=True;";
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>(eb =>
            {
                eb.Property(e => e.Email).IsRequired();
                
            });
            modelBuilder.Entity<Role>(eb =>
            {
                eb.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Dish>()
                .Property(d => d.Name)
                .IsRequired();

            modelBuilder.Entity<Address>(eb =>
            {
                eb.Property(r => r.City).HasMaxLength(50).IsRequired();
                eb.Property(r => r.Street).HasMaxLength(50).IsRequired();
                
            });

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
