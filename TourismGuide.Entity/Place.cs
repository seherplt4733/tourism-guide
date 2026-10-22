using System.ComponentModel.DataAnnotations.Schema;

namespace TourismGuide.Entity
{
    public class Place
    {
        public int Id { get; set; }

        // Veritabanındaki 'Name' sütununu C#'ta 'Title' olarak eşliyoruz
        // Hata almamak için 'string?' yaptık (Veritabanında null olabilir)
        [Column("Name")]
        public string? Title { get; set; }

        // C# kodunda Name istendiğinde Title dönsün (null ise boş string dönsün)
        [NotMapped]
        public string? Name 
        { 
            get => Title; 
            set => Title = value; 
        }

        // Veritabanında bu alanlar null olabileceği için başlarına ? ekledik
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? Address { get; set; }
        
        public decimal Price { get; set; }

        [NotMapped]
        public bool IsFree => Price == 0;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public double Rating { get; set; }
        
        public bool IsFeatured { get; set; } = false;

        // Navigation Properties
        public int CityId { get; set; }
        public City? City { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ItineraryPlace> ItineraryPlaces { get; set; } = new List<ItineraryPlace>();
    }
}