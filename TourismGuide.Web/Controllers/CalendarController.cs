using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /takvim
        [Route("takvim")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.City)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            ViewBag.Cities = await _context.Cities
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(events);
        }

        // GET: /api/etkinlikler-json
        [HttpGet("api/etkinlikler-json")]
        public async Task<IActionResult> GetEventsJson(int? cityId = null, string? search = null)
        {
            var query = _context.Events
                .Include(e => e.City)
                .AsQueryable();

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(e => e.CityId == cityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(s) || e.Location.ToLower().Contains(s));
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            var eventList = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.EventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.EventDate.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss"),
                url = $"/etkinlik-detay/{e.Id}",
                description = e.Description,
                location = e.Location,
                cityName = e.City?.Name ?? "Belirtilmedi",
                price = e.Price.HasValue ? (e.Price.Value == 0 ? "Ücretsiz" : $"{e.Price.Value:N0} ₺") : "Ücretsiz",
                imageUrl = !string.IsNullOrEmpty(e.ImageUrl) 
                    ? (e.ImageUrl.StartsWith("http") ? e.ImageUrl : "/img/" + System.IO.Path.GetFileName(e.ImageUrl))
                    : "/img/destination-1.jpg"
            });

            return Json(eventList);
        }
    }
}
