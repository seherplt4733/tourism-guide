using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Place)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        // POST or GET: /Admin/Reviews/Approve/5
        [HttpPost, HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum başarıyla onaylandı ve yayına alındı.";
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX POST: /Admin/Reviews/ApproveAjax/5
        [HttpPost]
        public async Task<IActionResult> ApproveAjax(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            review.IsApproved = true;
            await _context.SaveChangesAsync();

            var pendingCount = await _context.Reviews.CountAsync(r => !r.IsApproved);
            return Json(new
            {
                success = true,
                isApproved = true,
                pendingCount = pendingCount,
                message = "Yorum başarıyla onaylandı ve yayına alındı."
            });
        }

        // AJAX POST: /Admin/Reviews/ToggleStatusAjax/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatusAjax(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();

            var pendingCount = await _context.Reviews.CountAsync(r => !r.IsApproved);
            return Json(new
            {
                success = true,
                isApproved = review.IsApproved,
                pendingCount = pendingCount,
                message = review.IsApproved ? "Yorum başarıyla onaylandı ve yayına alındı." : "Yorum başarıyla askıya alındı / gizlendi."
            });
        }

        // POST or GET: /Admin/Reviews/ToggleStatus/5
        [HttpPost, HttpGet]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = review.IsApproved
                    ? "Yorum başarıyla onaylandı ve yayına alındı."
                    : "Yorum başarıyla askıya alındı / gizlendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST or GET: /Admin/Reviews/Delete/5
        [HttpPost, HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
