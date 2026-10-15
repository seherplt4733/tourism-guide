namespace TourismGuide.Entity
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }     // <-- Bu alanın tanımlı olduğundan emin olun
        public string? IconCssClass { get; set; }

        public List<Place> Places { get; set; } = new();
    }
}