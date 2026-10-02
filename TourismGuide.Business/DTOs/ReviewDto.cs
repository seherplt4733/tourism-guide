namespace TourismGuide.Business.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlaceId { get; set; }
    }
}