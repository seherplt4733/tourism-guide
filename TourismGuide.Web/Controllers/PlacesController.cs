using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;
using TourismGuide.Web.Models; // ViewModel'ini kullanabilmek için gerekli

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
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Geçersiz mekan Id.");
            }

            // 1. Ana mekanı veritabanından çek (Yorumlar, Şehir ve Kategori dahil)
            var place = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (place == null)
            {
                return NotFound($"Mekan bulunamadı: {id}");
            }

            // 2. Benzer mekanları bul (Aynı şehir ve aynı kategorideki diğer mekanlar)
            var similar = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Where(p => p.CityId == place.CityId
                         && p.CategoryId == place.CategoryId
                         && p.Id != place.Id)
                .Take(3)
                .ToListAsync();

            // 3. ViewModel'i doldur
            var vm = new PlaceDetailsViewModel
            {
                Place = place,
                SimilarPlaces = similar
            };

            return View(vm);
        }
        // POST: /Places/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review newReview)
        {
            ModelState.Remove("Place");
            if (!ModelState.IsValid)
            {
        // Form geçersizse detay sayfasını yeniden yükle
        return await Details(newReview.PlaceId);
    }

    newReview.CreatedAt = DateTime.Now;
    
    _context.Reviews.Add(newReview);
    await _context.SaveChangesAsync();

    // Yorum yapılan mekanın detay sayfasına geri dön
    return RedirectToAction(nameof(Details), new { id = newReview.PlaceId });
}
    }
}