using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourismGuide.Entity;

namespace TourismGuide.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
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
        public DbSet<Event> Events { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<EventAttendance> EventAttendances { get; set; }
        public DbSet<RoutePlan> RoutePlans { get; set; }
        public DbSet<RouteStop> RouteStops { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal Hassasiyeti
            modelBuilder.Entity<Place>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Event>()
                .Property(e => e.Price)
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
                .WithMany(p => p.ItineraryPlaces)
                .HasForeignKey(ip => ip.PlaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Veritabanında bulunmayan Order sütununu yoksay
            modelBuilder.Entity<ItineraryPlace>()
                .Ignore(ip => ip.Order);

            // Favorite İlişkileri
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Place)
                .WithMany()
                .HasForeignKey(f => f.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventAttendance İlişkileri
            modelBuilder.Entity<EventAttendance>()
                .HasOne(ea => ea.Event)
                .WithMany()
                .HasForeignKey(ea => ea.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventAttendance>()
                .HasOne(ea => ea.User)
                .WithMany()
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoutePlan & RouteStop İlişkileri
            modelBuilder.Entity<RoutePlan>()
                .HasOne(r => r.City)
                .WithMany()
                .HasForeignKey(r => r.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.RoutePlan)
                .WithMany(r => r.Stops)
                .HasForeignKey(rs => rs.RoutePlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.Place)
                .WithMany()
                .HasForeignKey(rs => rs.PlaceId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}