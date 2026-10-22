using Microsoft.AspNetCore.Identity;

namespace TourismGuide.Entity
{
    public class AppUser : IdentityUser
    {
        public string? ProfilePictureUrl { get; set; }
    }
}
