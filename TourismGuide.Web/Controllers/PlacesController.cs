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

        // URL: /mekan-detay/1, /mekan/1, /Places/Details/1, /Place/Detail/1
        [HttpGet]
        [Route("mekan-detay/{id}")]
        [Route("mekan/{id}")]
        [Route("Places/Details/{id}")]
        [Route("Place/Detail/{id}")]
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
        [Route("Places/AddReview")]
        [Route("Place/AddReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review newReview, IFormFile? reviewImage)
        {
            ModelState.Remove("Place");
            if (!ModelState.IsValid)
            {
                // Form geçersizse detay sayfasını yeniden yükle
                return await Details(newReview.PlaceId);
            }

            if (reviewImage != null && reviewImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(reviewImage.FileName).ToLowerInvariant();
                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reviews");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await reviewImage.CopyToAsync(stream);
                    }

                    newReview.ReviewImageUrl = $"/uploads/reviews/{uniqueFileName}";
                }
            }

            newReview.CreatedAt = DateTime.Now;
            newReview.IsApproved = false;
            
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            // Yorum yapılan mekanın detay sayfasına geri dön
            return RedirectToAction(nameof(Details), new { id = newReview.PlaceId });
        }
    }
}