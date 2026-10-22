namespace TourismGuide.Entity
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ankara, İstanbul vb.
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<Place> Places { get; set; } = new();
        public List<Itinerary> Itineraries { get; set; } = new();
        public List<Event> Events { get; set; } = new();
    }
}