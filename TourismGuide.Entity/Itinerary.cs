namespace TourismGuide.Entity
{
    public class Itinerary
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // örn: "1 Günde Ankara Turu"
        public string Description { get; set; } = null!;
        public int DurationHours { get; set; }
        
        public int CityId { get; set; }
        public City City { get; set; } = null!;

        public List<ItineraryPlace> ItineraryPlaces { get; set; } = new();
    }

    // Rota - Mekan Çoktan Çoğa Bağlantı Tablosu
    public class ItineraryPlace
    {
        public int ItineraryId { get; set; }
        public Itinerary Itinerary { get; set; } = null!;

        public int PlaceId { get; set; }
        public Place Place { get; set; } = null!;

        public int Order { get; set; } // Ziyaret Sırası (1. Anıtkabir, 2. Kale vb.)
    }
}