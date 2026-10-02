using Microsoft.EntityFrameworkCore;
using TourismGuide.Entity;


namespace TourismGuide.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Itinerary> Itineraries { get; set; }
        public DbSet<ItineraryPlace> ItineraryPlaces { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal Hassasiyeti
            modelBuilder.Entity<Place>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ItineraryPlace Kompozit Anahtar
            modelBuilder.Entity<ItineraryPlace>()
                .HasKey(ip => new { ip.ItineraryId, ip.PlaceId });

            modelBuilder.Entity<ItineraryPlace>()
                .HasOne(ip => ip.Itinerary)
                .WithMany(i => i.ItineraryPlaces)
                .HasForeignKey(ip => ip.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade Silme Dairesel Hatasını Engelleyen Kısım
            modelBuilder.Entity<ItineraryPlace>()
                .HasOne(ip => ip.Place)
                .WithMany()
                .HasForeignKey(ip => ip.PlaceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}