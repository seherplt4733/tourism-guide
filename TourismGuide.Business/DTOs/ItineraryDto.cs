namespace TourismGuide.Business.DTOs
{
    public class ItineraryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PlaceDto> Places { get; set; } = new();
    }
}