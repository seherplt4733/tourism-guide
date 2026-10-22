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
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CitiesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
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
            var cities = await _context.Cities
                .Include(c => c.Places)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(cities);
        }

        // GET: /Admin/Cities/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Cities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(City city, IFormFile? ImageFile, IFormFile? imageFile)
        {
            var file = ImageFile ?? imageFile;

            ModelState.Remove("Places");
            ModelState.Remove("Itineraries");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");

            var cityName = TextHelper.ToTitleCaseTurkish(city.Name);
            city.Name = cityName;

            if (!string.IsNullOrEmpty(cityName))
            {
                var existingCities = await _context.Cities.Select(c => c.Name).ToListAsync();
                var exists = existingCities.Any(n => string.Equals(n?.Trim(), cityName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Name", "Bu şehir zaten kayıtlı.");
                }
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    city.ImageUrl = uniqueFileName;
                }
                else if (string.IsNullOrWhiteSpace(city.ImageUrl))
                {
                    city.ImageUrl = "destination-1.jpg";
                }

                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Şehir başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        // GET: /Admin/Cities/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                TempData["ErrorMessage"] = "Şehir bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        // POST: /Admin/Cities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, City city, IFormFile? ImageFile, IFormFile? imageFile)
        {
            if (id != city.Id)
            {
                return BadRequest();
            }

            var file = ImageFile ?? imageFile;

            ModelState.Remove("Places");
            ModelState.Remove("Itineraries");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");

            var cityName = TextHelper.ToTitleCaseTurkish(city.Name);
            city.Name = cityName;

            if (!string.IsNullOrEmpty(cityName))
            {
                var existingCities = await _context.Cities.Where(c => c.Id != id).Select(c => c.Name).ToListAsync();
                var exists = existingCities.Any(n => string.Equals(n?.Trim(), cityName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Name", "Bu şehir zaten kayıtlı.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingCity = await _context.Cities.FirstOrDefaultAsync(c => c.Id == id);
                if (existingCity == null)
                {
                    TempData["ErrorMessage"] = "Şehir bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existingCity.Name = city.Name;
                existingCity.Description = city.Description ?? string.Empty;

                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    existingCity.ImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(city.ImageUrl))
                {
                    existingCity.ImageUrl = city.ImageUrl.Trim();
                }
                // Boş bırakılmışsa mevcut görsel korunur

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Şehir başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        // POST: /Admin/Cities/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Places)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city != null)
            {
                if (city.Places.Any())
                {
                    TempData["ErrorMessage"] = $"'{city.Name}' şehrine bağlı {city.Places.Count} adet mekan bulunduğu için silinemez. Önce bu mekana ait kayıtları düzenlemeli veya silmelisiniz.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Şehir başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek şehir bulunamadı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
