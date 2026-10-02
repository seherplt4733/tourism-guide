namespace TourismGuide.Entity
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Ankara, İstanbul vb.
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public List<Place> Places { get; set; } = new();
        public List<Itinerary> Itineraries { get; set; } = new();
    }
}