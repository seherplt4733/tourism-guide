using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Entity;
using TourismGuide.Web.Models;

namespace TourismGuide.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi ile kayıtlı bir hesap zaten var.");
                    return View(model);
                }

                // UserName alanına kullanıcının Ad Soyad bilgisini atıyoruz
                var user = new IdentityUser
                {
                    UserName = model.FullName,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Varsayılan olarak "User" rolü ver
                    await _userManager.AddToRoleAsync(user, "User");

                    // Kullanıcıyı otomatik giriş yaptır
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        /// POST: /Account/Login
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // 1. ÖNCELİK: Eğer kullanıcı Admin ise, Admin paneline gönder.
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Eğer kilit ikonundan gelmediyse bile admini panele at
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // 2. ÖNCELİK: Eğer bir ReturnUrl varsa oraya git.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // 3. ÖNCELİK: Normal kullanıcı ise Profil sayfasına gönder.
            return RedirectToAction("Profile", "Account");
        }

        ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
    }

    return View(model);
}

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout (alternatif direkt link için)
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int placeId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Favorilere eklemek için lütfen giriş yapın"
                });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Favorilere eklemek için lütfen giriş yapın"
                });
            }

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId);

            bool isFavorite;
            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                isFavorite = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    PlaceId = placeId,
                    CreatedAt = DateTime.Now
                });
                isFavorite = true;
            }

            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                isFavorite = isFavorite
            });
        }

        // GET: /Account/Favorites
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = await _context.Favorites
                .Include(f => f.Place)
                    .ThenInclude(p => p.City)
                .Include(f => f.Place)
                    .ThenInclude(p => p.Category)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favorites);
        }

        // GET: /Account/Profile
[Authorize]
[HttpGet]
public async Task<IActionResult> Profile()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login");
    }

    // Kullanıcının favorilerini çek
    var favorites = await _context.Favorites
        .Include(f => f.Place)
            .ThenInclude(p => p.City)
        .Include(f => f.Place)
            .ThenInclude(p => p.Category)
        .Where(f => f.UserId == user.Id)
        .OrderByDescending(f => f.CreatedAt)
        .ToListAsync();

    ViewBag.FavoriteCount = favorites.Count;
    ViewBag.Favorites = favorites; // View'da göstermek için
    ViewBag.UserEmail = user.Email;

    return View(user);
}

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
