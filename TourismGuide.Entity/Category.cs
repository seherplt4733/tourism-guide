namespace TourismGuide.Entity
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Müze, Park, Yeme-İçme, vb.
        public string IconCssClass { get; set; } = "fa-solid fa-location-dot"; // FontAwesome ikonu
        public List<Place> Places { get; set; } = new();
    }
}