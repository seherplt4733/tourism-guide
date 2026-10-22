using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.Business.Utilities;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PlacesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetWebRootPath()
        {
            var webRoot = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "TourismGuide.Web", "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
            }
            return webRoot;
        }

        public async Task<IActionResult> Index()
        {
            var places = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(places);
        }

        // GET: /Admin/Places/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        // POST: /Admin/Places/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Place place, IFormFile? ImageFile, IFormFile? imageFile)
        {
            var file = ImageFile ?? imageFile;

            ModelState.Remove("Name");
            ModelState.Remove("City");
            ModelState.Remove("Category");
            ModelState.Remove("Reviews");
            ModelState.Remove("ItineraryPlaces");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");

            // Tekillik / Duplicate Kontrolü: Aynı şehir altında aynı mekan adı olamaz (OrdinalIgnoreCase)
            var placeTitle = TextHelper.ToTitleCaseTurkish(place.Title);
            place.Title = placeTitle;
            place.Name = placeTitle;

            if (!string.IsNullOrEmpty(placeTitle))
            {
                var existingTitles = await _context.Places
                    .Where(p => p.CityId == place.CityId)
                    .Select(p => p.Title)
                    .ToListAsync();

                var exists = existingTitles.Any(t => string.Equals(t?.Trim(), placeTitle, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehirde bu isimde bir mekan zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsPlacesFolder = Path.Combine(webRoot, "uploads", "places");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsPlacesFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var placesPath = Path.Combine(uploadsPlacesFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, placesPath, true);
                    }
                    catch { }

                    place.ImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(place.ImageUrl))
                {
                    place.ImageUrl = place.ImageUrl.Trim();
                }

                _context.Places.Add(place);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mekan başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(place);
        }

        // GET: /Admin/Places/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                TempData["ErrorMessage"] = "Mekan bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(place);
        }

        // POST: /Admin/Places/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Place place, IFormFile? ImageFile, IFormFile? imageFile)
        {
            if (id != place.Id)
            {
                return BadRequest();
            }

            var file = ImageFile ?? imageFile;

            ModelState.Remove("Name");
            ModelState.Remove("City");
            ModelState.Remove("Category");
            ModelState.Remove("Reviews");
            ModelState.Remove("ItineraryPlaces");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");

            // Tekillik / Duplicate Kontrolü: Aynı şehir altında başka bir mekan aynı isme sahip olamaz (OrdinalIgnoreCase)
            var placeTitle = TextHelper.ToTitleCaseTurkish(place.Title);
            place.Title = placeTitle;
            place.Name = placeTitle;

            if (!string.IsNullOrEmpty(placeTitle))
            {
                var existingTitles = await _context.Places
                    .Where(p => p.Id != id && p.CityId == place.CityId)
                    .Select(p => p.Title)
                    .ToListAsync();

                var exists = existingTitles.Any(t => string.Equals(t?.Trim(), placeTitle, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehirde bu isimde bir mekan zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingPlace = await _context.Places.FirstOrDefaultAsync(p => p.Id == id);
                if (existingPlace == null)
                {
                    TempData["ErrorMessage"] = "Mekan bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existingPlace.Title = place.Title;
                existingPlace.CityId = place.CityId;
                existingPlace.CategoryId = place.CategoryId;
                existingPlace.Price = place.Price;
                existingPlace.Rating = place.Rating;
                if (place.Latitude != 0) existingPlace.Latitude = place.Latitude;
                if (place.Longitude != 0) existingPlace.Longitude = place.Longitude;
                existingPlace.Address = place.Address;
                existingPlace.ShortDescription = place.ShortDescription;
                existingPlace.FullDescription = place.FullDescription;
                existingPlace.IsFeatured = place.IsFeatured;

                // Görsel güncelleme: yeni resim seçilmişse kaydet ve ata
                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsPlacesFolder = Path.Combine(webRoot, "uploads", "places");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsPlacesFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var placesPath = Path.Combine(uploadsPlacesFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, placesPath, true);
                    }
                    catch { }

                    existingPlace.ImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(place.ImageUrl))
                {
                    existingPlace.ImageUrl = place.ImageUrl.Trim();
                }
                // Resim dosyası veya URL girilmemişse existingPlace.ImageUrl olduğu gibi korunur.

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mekan başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(place);
        }

        // POST: /Admin/Places/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place != null)
            {
                // İlgili favorileri ve yorumları doğrudan ID üzerinden güvenle sil
                var favorites = _context.Favorites.Where(f => f.PlaceId == id);
                _context.Favorites.RemoveRange(favorites);

                var reviews = _context.Reviews.Where(r => r.PlaceId == id);
                _context.Reviews.RemoveRange(reviews);

                _context.Places.Remove(place);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mekan başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek mekan bulunamadı.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /yonetim/mekanlar/toggle-featured/5
        [HttpPost("yonetim/mekanlar/toggle-featured/{id}")]
    
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place == null)
            {
                return Json(new { success = false, message = "Mekan bulunamadı." });
            }

            place.IsFeatured = !place.IsFeatured;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isFeatured = place.IsFeatured,
                message = place.IsFeatured ? $"\"{place.Name}\" öne çıkarıldı." : $"\"{place.Name}\" standart listeye alındı."
            });
        }
    }
}
