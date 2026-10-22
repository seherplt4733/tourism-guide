using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.Business.Utilities;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
            ViewBag.PendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);

            // 1. Şehirlere Göre Mekan Dağılımı (Doughnut Chart) - Büyük/küçük harf normalizasyonu
            var allCitiesWithPlaces = await _context.Cities
                .Include(c => c.Places)
                .Where(c => c.Places.Any())
                .ToListAsync();

            var cityPlaceData = allCitiesWithPlaces
                .GroupBy(c => (c.Name ?? string.Empty).Trim().ToLower())
                .Select(g => new
                {
                    CityName = TextHelper.ToTitleCaseTurkish(g.First().Name),
                    PlaceCount = g.Sum(c => c.Places.Count)
                })
                .OrderByDescending(x => x.PlaceCount)
                .Take(6)
                .ToList();

            if (!cityPlaceData.Any())
            {
                var defaultCities = await _context.Cities.Take(5).ToListAsync();
                cityPlaceData = defaultCities
                    .Select(c => new { CityName = TextHelper.ToTitleCaseTurkish(c.Name), PlaceCount = 0 })
                    .ToList();
            }

            ViewBag.CityPlaceLabelsJson = JsonSerializer.Serialize(cityPlaceData.Select(x => x.CityName));
            ViewBag.CityPlaceCountsJson = JsonSerializer.Serialize(cityPlaceData.Select(x => x.PlaceCount));

            // 2. Şehirlere Göre Yorum Yoğunluğu (Bar Chart) - Normalizasyon
            var allCitiesForReviews = await _context.Cities
                .Include(c => c.Places)
                .ToListAsync();

            var allReviews = await _context.Reviews
                .Include(r => r.Place)
                .ToListAsync();

            var cityReviewData = allCitiesForReviews
                .GroupBy(c => (c.Name ?? string.Empty).Trim().ToLower())
                .Select(g =>
                {
                    var cityIds = g.Select(c => c.Id).ToHashSet();
                    return new
                    {
                        CityName = TextHelper.ToTitleCaseTurkish(g.First().Name),
                        ReviewCount = allReviews.Count(r => r.Place != null && cityIds.Contains(r.Place.CityId))
                    };
                })
                .Where(x => x.ReviewCount > 0)
                .OrderByDescending(x => x.ReviewCount)
                .Take(6)
                .ToList();

            if (!cityReviewData.Any())
            {
                cityReviewData = cityPlaceData
                    .Select(x => new { CityName = x.CityName, ReviewCount = 0 })
                    .ToList();
            }

            ViewBag.CityReviewLabelsJson = JsonSerializer.Serialize(cityReviewData.Select(x => x.CityName));
            ViewBag.CityReviewCountsJson = JsonSerializer.Serialize(cityReviewData.Select(x => x.ReviewCount));

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
