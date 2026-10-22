using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;

namespace TourismGuide.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Messages
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View(messages);
        }

        // POST or GET: /Admin/Messages/MarkAsRead/5
        [HttpPost, HttpGet]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesaj okundu olarak işaretlendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Messages/MarkAsReadAjax/5
        [HttpPost]
        public async Task<IActionResult> MarkAsReadAjax(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // POST or GET: /Admin/Messages/Delete/5
        [HttpPost, HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesaj başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
