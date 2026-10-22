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
    [Route("yonetim/rotalar")]
    public class RoutesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoutesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
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

        // GET: /yonetim/rotalar
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var routes = await _context.RoutePlans
                .Include(r => r.City)
                .Include(r => r.Stops)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(routes);
        }

        // GET: /yonetim/rotalar/yeni
        [HttpGet("yeni")]
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Places = await _context.Places.OrderBy(p => p.Title).ToListAsync();

            var model = new RoutePlan
            {
                IsActive = true,
                EstimatedDuration = "4-5 Saat"
            };

            return View(model);
        }

        // POST: /yonetim/rotalar/yeni
        [HttpPost("yeni")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoutePlan routePlan, List<RouteStop>? stops, IFormFile? ImageFile)
        {
            ModelState.Remove("City");
            ModelState.Remove("ImageFile");
            ModelState.Remove("CoverImageUrl");

            if (stops != null)
            {
                for (int i = 0; i < stops.Count; i++)
                {
                    ModelState.Remove($"stops[{i}].RoutePlan");
                    ModelState.Remove($"stops[{i}].Place");
                }
            }

            // Tekillik Kontrolü & Title Case Standardı (OrdinalIgnoreCase)
            var title = TextHelper.ToTitleCaseTurkish(routePlan.Title);
            routePlan.Title = title;

            if (stops != null)
            {
                foreach (var stop in stops)
                {
                    if (!string.IsNullOrWhiteSpace(stop.CustomTitle))
                    {
                        stop.CustomTitle = TextHelper.ToTitleCaseTurkish(stop.CustomTitle);
                    }
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                var existingTitles = await _context.RoutePlans
                    .Where(r => r.CityId == routePlan.CityId)
                    .Select(r => r.Title)
                    .ToListAsync();

                var exists = existingTitles.Any(t => string.Equals(t?.Trim(), title, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehir için bu isimde bir gezi rotası zaten kayıtlı.");
                }
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsRoutesFolder = Path.Combine(webRoot, "uploads", "routes");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsRoutesFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var routesPath = Path.Combine(uploadsRoutesFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, routesPath, true);
                    }
                    catch { }

                    routePlan.CoverImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(routePlan.CoverImageUrl))
                {
                    routePlan.CoverImageUrl = routePlan.CoverImageUrl.Trim();
                }
                else
                {
                    routePlan.CoverImageUrl = "/img/destination-1.jpg";
                }

                // Durakları ekle
                if (stops != null && stops.Count > 0)
                {
                    int order = 1;
                    foreach (var s in stops)
                    {
                        if (s.PlaceId.HasValue || !string.IsNullOrWhiteSpace(s.CustomTitle))
                        {
                            s.StopOrder = order++;
                            routePlan.Stops.Add(s);
                        }
                    }
                }

                _context.RoutePlans.Add(routePlan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gezi rotası başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Places = await _context.Places.OrderBy(p => p.Title).ToListAsync();
            return View(routePlan);
        }

        // GET: /yonetim/rotalar/duzenle/{id}
        [HttpGet("duzenle/{id}")]
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var route = await _context.RoutePlans
                .Include(r => r.City)
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .ThenInclude(s => s.Place)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                TempData["ErrorMessage"] = "Rota bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Places = await _context.Places.OrderBy(p => p.Title).ToListAsync();
            return View(route);
        }

        // POST: /yonetim/rotalar/duzenle/{id}
        [HttpPost("duzenle/{id}")]
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoutePlan routePlan, List<RouteStop>? stops, IFormFile? ImageFile)
        {
            if (id != routePlan.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("City");
            ModelState.Remove("ImageFile");
            ModelState.Remove("CoverImageUrl");

            if (stops != null)
            {
                for (int i = 0; i < stops.Count; i++)
                {
                    ModelState.Remove($"stops[{i}].RoutePlan");
                    ModelState.Remove($"stops[{i}].Place");
                }
            }

            // Tekillik Kontrolü & Title Case Standardı (OrdinalIgnoreCase)
            var title = TextHelper.ToTitleCaseTurkish(routePlan.Title);
            routePlan.Title = title;

            if (stops != null)
            {
                foreach (var stop in stops)
                {
                    if (!string.IsNullOrWhiteSpace(stop.CustomTitle))
                    {
                        stop.CustomTitle = TextHelper.ToTitleCaseTurkish(stop.CustomTitle);
                    }
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                var existingTitles = await _context.RoutePlans
                    .Where(r => r.Id != id && r.CityId == routePlan.CityId)
                    .Select(r => r.Title)
                    .ToListAsync();

                var exists = existingTitles.Any(t => string.Equals(t?.Trim(), title, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Title", "Bu şehir için bu isimde bir gezi rotası zaten kayıtlı.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingRoute = await _context.RoutePlans
                    .Include(r => r.Stops)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (existingRoute == null)
                {
                    TempData["ErrorMessage"] = "Rota bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existingRoute.Title = routePlan.Title;
                existingRoute.CityId = routePlan.CityId;
                existingRoute.EstimatedDuration = routePlan.EstimatedDuration;
                existingRoute.TotalDistance = routePlan.TotalDistance;
                existingRoute.Summary = routePlan.Summary;
                existingRoute.IsActive = routePlan.IsActive;

                // Görsel güncelleme
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var webRoot = GetWebRootPath();
                    var uploadsFolder = Path.Combine(webRoot, "img");
                    var uploadsRoutesFolder = Path.Combine(webRoot, "uploads", "routes");

                    Directory.CreateDirectory(uploadsFolder);
                    Directory.CreateDirectory(uploadsRoutesFolder);

                    var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    try
                    {
                        var routesPath = Path.Combine(uploadsRoutesFolder, uniqueFileName);
                        System.IO.File.Copy(filePath, routesPath, true);
                    }
                    catch { }

                    existingRoute.CoverImageUrl = uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(routePlan.CoverImageUrl))
                {
                    existingRoute.CoverImageUrl = routePlan.CoverImageUrl.Trim();
                }

                // Durakları güncelle (Mevcut durakları temizle ve yenilerini ekle)
                _context.RouteStops.RemoveRange(existingRoute.Stops);

                if (stops != null && stops.Count > 0)
                {
                    int order = 1;
                    foreach (var s in stops)
                    {
                        if (s.PlaceId.HasValue || !string.IsNullOrWhiteSpace(s.CustomTitle))
                        {
                            existingRoute.Stops.Add(new RouteStop
                            {
                                RoutePlanId = id,
                                StopOrder = order++,
                                PlaceId = s.PlaceId,
                                CustomTitle = s.CustomTitle,
                                SuggestedTime = s.SuggestedTime,
                                TransportationTip = s.TransportationTip,
                                Note = s.Note
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gezi rotası başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Places = await _context.Places.OrderBy(p => p.Title).ToListAsync();
            return View(routePlan);
        }

        // POST: /yonetim/rotalar/sil/{id}
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

            var route = await _context.RoutePlans
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == targetId.Value);

            if (route != null)
            {
                _context.RoutePlans.Remove(route);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gezi rotası ve bağlı tüm durakları başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek rota bulunamadı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
