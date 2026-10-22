using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Events
        public IActionResult Index()
        {
            return Redirect("/takvim");
        }

        // GET: /Events/Calendar
        [Route("etkinlik-takvimi")]
        public IActionResult Calendar()
        {
            return Redirect("/takvim");
        }

        // GET: /Events/GetEvents
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.City)
                .ToListAsync();

            var eventList = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.EventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                url = $"/etkinlik-detay/{e.Id}",
                description = e.Description,
                location = e.Location,
                cityName = e.City?.Name,
                price = e.Price,
                imageUrl = e.ImageUrl
            });

            return Json(eventList);
        }

        // GET: /etkinlik-detay/5
        [HttpGet]
        [Route("etkinlik-detay/{id}")]
        [Route("Events/Detail/{id}")]
        [Route("Events/Details/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var ev = await _context.Events
                .Include(e => e.City)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound("Etkinlik bulunamadı.");
            }

            bool isAttending = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    isAttending = await _context.EventAttendances
                        .AnyAsync(ea => ea.UserId == userId && ea.EventId == id);
                }
            }

            // Mekan eşleşmesi kontrolü (Konum ismi veya şehirde benzer isimli mekan)
            Place? matchingPlace = null;
            if (!string.IsNullOrWhiteSpace(ev.Location))
            {
                var loc = ev.Location.Trim();
                matchingPlace = await _context.Places
                    .Include(p => p.City)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.CityId == ev.CityId && 
                        (p.Title!.Contains(loc) || loc.Contains(p.Title!)));

                if (matchingPlace == null)
                {
                    matchingPlace = await _context.Places
                        .Include(p => p.City)
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.Title!.Contains(loc) || loc.Contains(p.Title!));
                }
            }

            // Diğer yaklaşan etkinlikler
            var otherEvents = await _context.Events
                .Include(e => e.City)
                .Where(e => e.Id != id && e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();

            ViewBag.IsAttending = isAttending;
            ViewBag.MatchingPlace = matchingPlace;
            ViewBag.OtherEvents = otherEvents;

            return View("Detail", ev);
        }

        public async Task<IActionResult> Details(int id)
        {
            return await Detail(id);
        }
    }
}
