namespace TourismGuide.Business.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public string? ReviewImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlaceId { get; set; }
    }
}