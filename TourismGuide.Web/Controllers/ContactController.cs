using Microsoft.AspNetCore.Mvc;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                model.IsRead = false;
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesajınız başarıyla iletildi.";
            }
            return Redirect("/#contact");
        }
    }
}
