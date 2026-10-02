using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Web.Models;

namespace TourismGuide.Web.Controllers
{
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // URL: /places/details/1
        // veya Razor'da: asp-controller="Places" asp-action="Details" asp-route-id="1"
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Geçersiz mekan Id.");
            }

            var place = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (place == null)
            {
                return NotFound($"Mekan bulunamadı: {id}");
            }

            var similar = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Where(p => p.CityId == place.CityId
                         && p.CategoryId == place.CategoryId
                         && p.Id != place.Id)
                .Take(3)
                .ToListAsync();

            var vm = new PlaceDetailsViewModel
            {
                Place = place,
                SimilarPlaces = similar
            };

            return View(vm);
        }
    }
}