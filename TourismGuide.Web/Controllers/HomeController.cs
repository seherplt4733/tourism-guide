using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        [Route("Home")]
        [Route("Home/Index")]
        [Route("mekanlar")]
        public async Task<IActionResult> Index(string? searchKeyword, string? search, int? categoryId)
        {
            var activeKeyword = !string.IsNullOrWhiteSpace(search) ? search : searchKeyword;
            ViewBag.Categories = await _context.Categories.ToListAsync() ?? new List<Category>();
            ViewBag.Cities = await _context.Cities.ToListAsync() ?? new List<City>();
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.SearchKeyword = activeKeyword;

            // Tarihi geçmemiş en yakın 6 etkinliği çek
            var now = DateTime.Now;
            ViewBag.UpcomingEvents = await _context.Events
                .Include(e => e.City)
                .Where(e => e.EventDate >= now.Date)
                .OrderBy(e => e.EventDate)
                .Take(6)
                .ToListAsync() ?? new List<Event>();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    ViewBag.UserFavoritePlaceIds = await _context.Favorites
                        .Where(f => f.UserId == userId)
                        .Select(f => f.PlaceId)
                        .ToListAsync() ?? new List<int>();

                    ViewBag.UserAttendedEventIds = await _context.EventAttendances
                        .Where(ea => ea.UserId == userId)
                        .Select(ea => ea.EventId)
                        .ToListAsync() ?? new List<int>();
                }
            }

            var query = _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(activeKeyword))
            {
                var trimmed = activeKeyword.Trim();
                query = query.Where(p =>
                    (p.Title != null && p.Title.Contains(trimmed)) ||
                    (p.City != null && p.City.Name != null && p.City.Name.Contains(trimmed)) ||
                    (p.Category != null && p.Category.Name != null && p.Category.Name.Contains(trimmed)) ||
                    (p.ShortDescription != null && p.ShortDescription.Contains(trimmed)) ||
                    (p.FullDescription != null && p.FullDescription.Contains(trimmed)));
            }

            // Kategori filtresi
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var resultList = await query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.Rating)
                .ThenByDescending(p => p.Id)
                .ToListAsync();
            return View(resultList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}