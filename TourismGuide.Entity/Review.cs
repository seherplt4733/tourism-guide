namespace TourismGuide.Entity
{
    public class Review
    {
        public int Id { get; set; }

        public string Comment { get; set; } = string.Empty;

        public int Rating { get; set; } // 1 - 5 arası puan

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler (Foreign Key & Navigation Properties)
        public int PlaceId { get; set; }
        public Place? Place { get; set; }

        // İleride Identity / User eklediğinde buraya UserId de bağlayabilirsin
        // public string UserId { get; set; }
    }
}