using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PlacesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Mekanları Listeleme
        public IActionResult Index()
        {
            var places = _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .ToList();

            return View(places);
        }

        // Yeni Mekan Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Cities = new SelectList(_context.Cities.ToList(), "Id", "Name");
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            return View();
        }

        // Yeni Mekan Kaydetme (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Place place, IFormFile? imageFile)
        {
            // EF Core doğrulamasına takılacak ilişki ve nav mekanizmalarını temizliyoruz
            ModelState.Remove("City");
            ModelState.Remove("Category");
            ModelState.Remove("Reviews");
            ModelState.Remove("Itineraries");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ShortDescription");
            ModelState.Remove("FullDescription");

            // Mekan adı/başlığı boş değilse işleme devam et
            if (!string.IsNullOrWhiteSpace(place.Name) || !string.IsNullOrWhiteSpace(place.Title))
            {
                // Name / Title uyumsuzluğunu güvene alıyoruz
                if (string.IsNullOrEmpty(place.Title) && !string.IsNullOrEmpty(place.Name))
                    place.Title = place.Name;
                if (string.IsNullOrEmpty(place.Name) && !string.IsNullOrEmpty(place.Title))
                    place.Name = place.Title;

                place.ShortDescription ??= "";
                place.FullDescription ??= "";

                // Eğer bir resim dosyası seçildiyse wwwroot/img klasörüne kaydet
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, "img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    place.ImageUrl = "/img/" + fileName;
                }
                else if (string.IsNullOrEmpty(place.ImageUrl))
                {
                    place.ImageUrl = "/img/destination-1.jpg"; // Varsayılan görsel
                }

                _context.Places.Add(place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = new SelectList(_context.Cities.ToList(), "Id", "Name", place.CityId);
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", place.CategoryId);
            return View(place);
        }

        // Mekan Silme Action
        public IActionResult Delete(int id)
        {
            var place = _context.Places.Find(id);
            if (place != null)
            {
                var reviews = _context.Reviews.Where(r => r.PlaceId == id).ToList();
                if (reviews.Any())
                {
                    _context.Reviews.RemoveRange(reviews);
                }

                _context.Places.Remove(place);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}