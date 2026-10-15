namespace TourismGuide.Entity
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Şehir adı zorunlu
        public string? Description { get; set; }  // İsteğe bağlı
        public string? ImageUrl { get; set; }     // İsteğe bağlı

        public List<Place> Places { get; set; } = new();
        public List<Itinerary> Itineraries { get; set; } = new();
    }
}