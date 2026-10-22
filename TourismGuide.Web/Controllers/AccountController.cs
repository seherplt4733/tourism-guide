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
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
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
                var user = new AppUser
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

        // GET: /Account/Login & /yonetim/giris & /giris-yap
        [HttpGet]
        [Route("Account/Login")]
        [Route("giris-yap")]
        [Route("yonetim/giris")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (Request.Path.Value?.Contains("yonetim/giris") == true && string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/yonetim";
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                return RedirectToAction("Profile", "Account");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login & /yonetim/giris & /giris-yap
        [HttpPost]
        [Route("Account/Login")]
        [Route("giris-yap")]
        [Route("yonetim/giris")]
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
                    // 1. Eğer kullanıcı "Admin" rolündeyse, doğrudan Admin paneline yönlendir
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    // 2. Eğer bir ReturnUrl varsa oraya git (Normal kullanıcıların /yonetim'e yönlenmesini engelle)
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        if (model.ReturnUrl.StartsWith("/yonetim", StringComparison.OrdinalIgnoreCase))
                        {
                            return RedirectToAction("Profile", "Account");
                        }
                        return Redirect(model.ReturnUrl);
                    }

                    // 3. Normal kullanıcı ise Profil sayfasına gönder
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

        // POST: /Account/ToggleFavorite & /favori-toggle/{placeId}
        [HttpPost]
        [Route("Account/ToggleFavorite")]
        [Route("favori-toggle/{placeId}")]
        [Route("/favori-toggle/{placeId}")]
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
                isFavorite = isFavorite,
                message = isFavorite ? "Mekan favorilerinize eklendi." : "Mekan favorilerinizden kaldırıldı."
            });
        }

        // POST: /Account/ToggleAttendance
        [HttpPost]
        public async Task<IActionResult> ToggleAttendance(int eventId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Etkinliğe katılmak için lütfen giriş yapın"
                });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Etkinliğe katılmak için lütfen giriş yapın"
                });
            }

            var existing = await _context.EventAttendances
                .FirstOrDefaultAsync(ea => ea.UserId == userId && ea.EventId == eventId);

            bool isAttending;
            if (existing != null)
            {
                _context.EventAttendances.Remove(existing);
                isAttending = false;
            }
            else
            {
                _context.EventAttendances.Add(new EventAttendance
                {
                    UserId = userId,
                    EventId = eventId,
                    CreatedAt = DateTime.Now
                });
                isAttending = true;
            }

            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                isAttending = isAttending
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

            // Kullanıcının yorumlarını çek
            var reviews = await _context.Reviews
                .Include(r => r.Place)
                    .ThenInclude(p => p.City)
                .Where(r => r.ReviewerName == user.UserName || r.Email == user.Email || r.Email == user.UserName)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Kullanıcının katıldığı etkinlikleri çek
            var attendances = await _context.EventAttendances
                .Include(ea => ea.Event)
                    .ThenInclude(e => e.City)
                .Where(ea => ea.UserId == user.Id)
                .OrderByDescending(ea => ea.CreatedAt)
                .ToListAsync();

            ViewBag.FavoriteCount = favorites.Count;
            ViewBag.Favorites = favorites;
            ViewBag.Reviews = reviews;
            ViewBag.ReviewCount = reviews.Count;
            ViewBag.Attendances = attendances;
            ViewBag.AttendanceCount = attendances.Count;
            ViewBag.UserEmail = user.Email;

            return View(user);
        }

        // POST: /Account/UploadProfilePicture
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Yalnızca resim dosyaları (.jpg, .jpeg, .png, .webp) yüklenebilir.";
                    return RedirectToAction(nameof(Profile));
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "Profil fotoğrafınız başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
