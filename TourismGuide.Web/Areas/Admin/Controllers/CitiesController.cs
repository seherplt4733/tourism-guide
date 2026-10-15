using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Şehir Listesi
        public IActionResult Index()
        {
            var cities = _context.Cities.ToList();
            return View(cities);
        }

        // Yeni Şehir Sayfası (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Şehir Kaydet (POST)
        [HttpPost]
        public IActionResult Create(City city)
        {
            // ModelState içindeki zorunlu olmayan alan kontrolünü kaldırıyoruz
            ModelState.Remove("Description");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Places");
            ModelState.Remove("Itineraries");

            if (!string.IsNullOrWhiteSpace(city.Name))
            {
                city.Description ??= "";
                city.ImageUrl ??= "/img/destination-1.jpg";

                _context.Cities.Add(city);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Şehir başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("Name", "Şehir adı boş bırakılamaz.");
            return View(city);
        }

        // Şehir Silme
        public IActionResult Delete(int id)
        {
            var city = _context.Cities.Find(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Şehir başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}