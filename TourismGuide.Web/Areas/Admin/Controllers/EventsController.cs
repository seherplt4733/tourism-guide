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
    [Route("yonetim/etkinlikler")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
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

        // GET: /yonetim/etkinlikler
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.City)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
            return View(events);
        }

        // GET: /yonetim/etkinlikler/yeni
        [HttpGet("yeni")]
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        // POST: /yonetim/etkinlikler/yeni
        [HttpPost("yeni")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event, IFormFile? ImageFile, IFormFile? imageFile)
        {
            var file = ImageFile ?? imageFile;

            ModelState.Remove("City");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");

            // Tekillik / Duplicate Kontrolü: Aynı Şehir + Aynı Konum/Mekan + Aynı Etkinlik Adı (OrdinalIgnoreCase)
            var title = TextHelper.ToTitleCaseTurkish(@event.Title);
            @event.Title = title;
            var location = TextHelper.ToTitleCaseTurkish(@event.Location);
            @event.Location = location;

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(location))
            {
                var existingEvents = await _context.Events
                    .Where(e => e.CityId == @event.CityId)
                    .Select(e => new { e.Title, e.Location })
                    .ToListAsync();

                var exists = existingEvents.Any(e =>
                    string.Equals(e.Title?.Trim(), title, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.Location?.Trim(), location, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehir ve mekanda bu isimle bir etkinlik zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsEventsFolder = Path.Combine(webRoot, "uploads", "events");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsEventsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var eventsPath = Path.Combine(uploadsEventsFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, eventsPath, true);
                    }
                    catch { }

                    @event.ImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(@event.ImageUrl))
                {
                    @event.ImageUrl = @event.ImageUrl.Trim();
                }
                else
                {
                    @event.ImageUrl = "/img/destination-1.jpg";
                }

                _context.Events.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View(@event);
        }

        // GET: /yonetim/etkinlikler/duzenle/{id}
        [HttpGet("duzenle/{id}")]
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View(@event);
        }

        // POST: /yonetim/etkinlikler/duzenle/{id}
        [HttpPost("duzenle/{id}")]
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event, IFormFile? ImageFile, IFormFile? imageFile)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            var file = ImageFile ?? imageFile;

            ModelState.Remove("City");
            ModelState.Remove("ImageFile");
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageUrl");

            // Tekillik / Duplicate Kontrolü: Aynı Şehir + Aynı Konum/Mekan + Aynı Etkinlik Adı (OrdinalIgnoreCase)
            var title = TextHelper.ToTitleCaseTurkish(@event.Title);
            @event.Title = title;
            var location = TextHelper.ToTitleCaseTurkish(@event.Location);
            @event.Location = location;

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(location))
            {
                var existingEvents = await _context.Events
                    .Where(e => e.Id != id && e.CityId == @event.CityId)
                    .Select(e => new { e.Title, e.Location })
                    .ToListAsync();

                var exists = existingEvents.Any(e =>
                    string.Equals(e.Title?.Trim(), title, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.Location?.Trim(), location, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehir ve mekanda bu isimle bir etkinlik zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
                if (existingEvent == null)
                {
                    TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existingEvent.Title = @event.Title;
                existingEvent.CityId = @event.CityId;
                existingEvent.EventDate = @event.EventDate;
                existingEvent.Price = @event.Price;
                existingEvent.Location = @event.Location;
                existingEvent.Description = @event.Description;

                // Görsel güncelleme: yeni dosya seçilmişse kaydet ve ata
                if (file != null && file.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsEventsFolder = Path.Combine(webRoot, "uploads", "events");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsEventsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var eventsPath = Path.Combine(uploadsEventsFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, eventsPath, true);
                    }
                    catch { }

                    existingEvent.ImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(@event.ImageUrl))
                {
                    existingEvent.ImageUrl = @event.ImageUrl.Trim();
                }
                // Yeni görsel seçilmediyse veya URL girilmediyse mevcut ImageUrl korunur!

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View(@event);
        }

        // POST: /yonetim/etkinlikler/sil/{id}
        [HttpPost("sil/{id?}")]
        [HttpPost("Delete/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id, [FromForm] int? formId)
        {
            var targetId = id ?? formId;
            if (!targetId.HasValue)
            {
                return BadRequest();
            }

            var @event = await _context.Events.FindAsync(targetId.Value);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
