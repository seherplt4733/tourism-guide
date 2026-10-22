using System;

namespace TourismGuide.Entity
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int CityId { get; set; }
        public virtual City? City { get; set; }
    }
}
