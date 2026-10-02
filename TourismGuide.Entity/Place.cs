using System.ComponentModel.DataAnnotations.Schema;

namespace TourismGuide.Entity
{
    public class Place
    {
        public int Id { get; set; }

        // Veritabanındaki 'Name' sütununu C#'ta 'Title' olarak eşliyoruz
        [Column("Name")]
        public string Title { get; set; } = string.Empty;

        // C# kodunda Name istendiğinde de Title dönsün
        [NotMapped]
        public string Name 
        { 
            get => Title; 
            set => Title = value; 
        }

        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [NotMapped]
        public bool IsFree => Price == 0;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }

        // Navigation Properties
        public int CityId { get; set; }
        public City? City { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ItineraryPlace> ItineraryPlaces { get; set; } = new List<ItineraryPlace>();
    }
}