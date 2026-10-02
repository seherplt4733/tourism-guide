using TourismGuide.Entity;

namespace TourismGuide.Web.Models
{
    public class PlaceDetailsViewModel
    {
        public Place Place { get; set; } = null!;
        public List<Place> SimilarPlaces { get; set; } = new();
    }
}