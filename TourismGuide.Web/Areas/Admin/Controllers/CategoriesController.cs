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
    [Route("yonetim/kategoriler")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /yonetim/kategoriler
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Places)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /yonetim/kategoriler/yeni
        [HttpGet("yeni")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /yonetim/kategoriler/yeni
        [HttpPost("yeni")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ModelState.Remove("Places");

            var categoryName = TextHelper.ToTitleCaseTurkish(category.Name);
            category.Name = categoryName;

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                ModelState.AddModelError("Name", "Kategori adı zorunludur.");
            }
            else
            {
                // Tekillik / Duplicate Kontrolü (Case-insensitive StringComparison.OrdinalIgnoreCase)
                var existingCats = await _context.Categories.Select(c => c.Name).ToListAsync();
                var exists = existingCats.Any(n => string.Equals(n?.Trim(), categoryName, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    ModelState.AddModelError("Name", "Bu kategori zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                category.Name = categoryName;
                if (!string.IsNullOrWhiteSpace(category.IconCssClass))
                {
                    category.IconCssClass = category.IconCssClass.Trim();
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /yonetim/kategoriler/duzenle/5
        [HttpGet("duzenle/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: /yonetim/kategoriler/duzenle/5
        [HttpPost("duzenle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("Places");

            var categoryName = TextHelper.ToTitleCaseTurkish(category.Name);
            category.Name = categoryName;

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                ModelState.AddModelError("Name", "Kategori adı zorunludur.");
            }
            else
            {
                // Tekillik / Duplicate Kontrolü: Aynı isimde başka kategori var mı?
                var existingCats = await _context.Categories.Where(c => c.Id != id).Select(c => c.Name).ToListAsync();
                var exists = existingCats.Any(n => string.Equals(n?.Trim(), categoryName, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    ModelState.AddModelError("Name", "Bu kategori zaten mevcut.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (existingCategory == null)
                {
                    TempData["ErrorMessage"] = "Kategori bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existingCategory.Name = categoryName;
                existingCategory.IconCssClass = !string.IsNullOrWhiteSpace(category.IconCssClass) 
                    ? category.IconCssClass.Trim() 
                    : null;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: /yonetim/kategoriler/sil/5
        [HttpPost("sil/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Places)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Bağlı mekan kontrolü: Eğer kategoriye bağlı mekan varsa silmeyi engelle!
            if (category.Places != null && category.Places.Any())
            {
                TempData["ErrorMessage"] = $"'{category.Name}' kategorisine bağlı {category.Places.Count} adet mekan bulunmaktadır. Bu kategoriyi silebilmek için önce bağlı mekanları silmeli veya başka bir kategoriye taşımalısınız.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
