using System;
using Microsoft.AspNetCore.Identity;

namespace TourismGuide.Entity
{
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual AppUser? User { get; set; }

        public int PlaceId { get; set; }
        public virtual Place? Place { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
