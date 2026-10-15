using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kategori Listeleme
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // Yeni Kategori Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Kategori Kaydet (POST)
        [HttpPost]
        public IActionResult Create(Category category)
        {
            // İlişkili nesnelerin doğrulamaya takılmaması için
            ModelState.Remove("Places");

            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                _context.Categories.Add(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("Name", "Kategori adı boş bırakılamaz.");
            return View(category);
        }

        // Kategori Düzenleme Sayfası (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Kategori Güncelleme (POST)
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            ModelState.Remove("Places");

            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                _context.Categories.Update(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // Kategori Silme
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}