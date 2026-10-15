using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Yorum Listesi
        public IActionResult Index()
        {
            var reviews = _context.Reviews
                .Include(r => r.Place)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(reviews);
        }

        // Yorumu Onayla veya Onayı Kaldır (Toggle)
        public IActionResult ToggleApprove(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                _context.SaveChanges();

                TempData["SuccessMessage"] = review.IsApproved 
                    ? "Yorum onaylandı ve sitede yayınlandı." 
                    : "Yorum askıya alındı ve siteden gizlendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Yorum Silme
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}