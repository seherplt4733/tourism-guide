using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mekan Detay Sayfası
        public IActionResult Details(int id)
        {
            var place = _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Include(p => p.Reviews.Where(r => r.IsApproved)) // Yalnızca onaylı yorumları getirir
                .FirstOrDefault(p => p.Id == id);

            if (place == null)
            {
                return NotFound();
            }

            return View(place);
        }

        // Yorum Ekleme Aksiyonu (POST)
        [HttpPost]
        public IActionResult AddReview(int placeId, string reviewerName, string comment, int rating)
        {
            if (!string.IsNullOrWhiteSpace(comment) && rating >= 1 && rating <= 5)
            {
                var review = new Review
                {
                    PlaceId = placeId,
                    ReviewerName = string.IsNullOrWhiteSpace(reviewerName) ? "Anonim Ziyaretçi" : reviewerName,
                    Comment = comment,
                    Rating = rating,
                    CreatedAt = DateTime.Now,
                    IsApproved = false // Otomatik olarak onay bekleyen modda kaydedilir
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();

                TempData["ReviewMessage"] = "Yorumunuz alındı! Yöneticimiz inceledikten sonra yayınlanacaktır.";
            }

            return RedirectToAction(nameof(Details), new { id = placeId });
        }
    }
}