namespace TourismGuide.Business.DTOs
{
    public class PlaceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}