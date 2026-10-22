using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Controllers
{
    public class RoutesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /rotalar
        [Route("rotalar")]
        public async Task<IActionResult> Index(int? cityId)
        {
            var query = _context.RoutePlans
                .Include(r => r.City)
                .Include(r => r.Stops)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(r => r.CityId == cityId.Value);
            }

            var routes = await query.OrderByDescending(r => r.Id).ToListAsync();

            ViewBag.Cities = await _context.Cities
                .Where(c => _context.RoutePlans.Any(r => r.CityId == c.Id && r.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CurrentCityId = cityId;

            return View(routes);
        }

        // GET: /rota-detay/5
        [Route("rota-detay/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var route = await _context.RoutePlans
                .Include(r => r.City)
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .ThenInclude(s => s.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                return NotFound("Gezi rotası bulunamadı.");
            }

            return View(route);
        }
    }
}
