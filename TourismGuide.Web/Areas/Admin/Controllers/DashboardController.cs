using Microsoft.AspNetCore.Mvc;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
  [Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // İstatistik Verileri
            ViewBag.TotalPlaces = _context.Places.Count();
            ViewBag.TotalCities = _context.Cities.Count();
            ViewBag.TotalCategories = _context.Categories.Count();
            ViewBag.TotalReviews = _context.Reviews.Count();

            return View();
        }
    }
}