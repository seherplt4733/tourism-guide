namespace TourismGuide.Entity
{
    public class Review
    {
        public int Id { get; set; }

        // YENİ: Yorumu yapan kişinin adı
        public string ReviewerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Comment { get; set; } = string.Empty;

        public int Rating { get; set; } // 1 - 5 arası puan

        public bool IsApproved { get; set; } = false;

        public string? ReviewImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public int PlaceId { get; set; }
        public Place? Place { get; set; }
    }
}