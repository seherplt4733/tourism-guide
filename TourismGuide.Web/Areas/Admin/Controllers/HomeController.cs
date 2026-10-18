using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalPlaces = await _context.Places.CountAsync();
            ViewBag.TotalCities = await _context.Cities.CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.CountAsync();
            ViewBag.TotalEvents = await _context.Events.CountAsync();
            ViewBag.PendingReviews = 0; // Onay bekleyen yorum

            ViewBag.RecentPlaces = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentReviews = await _context.Reviews
                .Include(r => r.Place)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingEvents = await _context.Events
                .Include(e => e.City)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
