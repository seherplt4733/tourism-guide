using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchKeyword, int? categoryId)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.SearchKeyword = searchKeyword;

            // Tarihi geçmemiş en yakın 6 etkinliği çek
            var now = DateTime.Now;
            ViewBag.UpcomingEvents = await _context.Events
                .Include(e => e.City)
                .Where(e => e.EventDate >= now.Date)
                .OrderBy(e => e.EventDate)
                .Take(6)
                .ToListAsync();

            var query = _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchKeyword))
{
    query = query.Where(p =>
        p.Title.Contains(searchKeyword) ||  // Name yerine Title kullan
        (p.City != null && p.City.Name.Contains(searchKeyword)) ||
        (p.Category != null && p.Category.Name.Contains(searchKeyword)));
}

            // Kategori filtresi
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var resultList = await query.ToListAsync();
            return View(resultList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}