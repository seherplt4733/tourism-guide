namespace TourismGuide.Entity
{
    public class Category
    {
        public int Id { get; set; }
        
        // Başlarına ? (soru işareti) ekledik. Artık NULL gelirse hata vermez.
        public string? Name { get; set; } 
        public string? IconCssClass { get; set; } 
        
        public List<Place> Places { get; set; } = new();
    }
}