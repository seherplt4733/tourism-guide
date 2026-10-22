using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourismGuide.DataAccess;
using TourismGuide.Business.Services;
using TourismGuide.Entity;
using TourismGuide.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ abcdefghijklmnopqrstuvwxyzçğıöşüÇĞİÖŞÜ ";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/giris-yap";
    options.LogoutPath = "/cikis-yap";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPlaceService, PlaceService>();

var app = builder.Build();

// Seed Admin & Roles ("Yalnızca Tek Admin" Kuralı)
await DbInitializer.SeedAdminAndRolesAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- TÜRKÇE SEO & CUSTOM ROUTING ---

// 1. Admin Paneli Türkçe Rotaları
app.MapControllerRoute(
    name: "admin_places",
    pattern: "yonetim/mekanlar/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Places" });

app.MapControllerRoute(
    name: "admin_cities",
    pattern: "yonetim/sehirler/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Cities" });

app.MapControllerRoute(
    name: "admin_categories",
    pattern: "yonetim/kategoriler/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Categories" });

app.MapControllerRoute(
    name: "admin_reviews",
    pattern: "yonetim/yorumlar/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Reviews" });

app.MapControllerRoute(
    name: "admin_messages",
    pattern: "yonetim/gelen-mesajlar/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Messages" });

app.MapControllerRoute(
    name: "admin_events",
    pattern: "yonetim/etkinlikler/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Events" });

app.MapControllerRoute(
    name: "admin_routes",
    pattern: "yonetim/rotalar/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Routes" });

app.MapControllerRoute(
    name: "admin_dashboard",
    pattern: "yonetim/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "Home" });

// 2. Kullanıcı Arayüzü Türkçe Rotaları
app.MapControllerRoute(
    name: "admin_login",
    pattern: "yonetim/giris",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "places_list",
    pattern: "mekanlar",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "place_details",
    pattern: "mekan-detay/{id}",
    defaults: new { controller = "Places", action = "Details" });

app.MapControllerRoute(
    name: "place_short",
    pattern: "mekan/{id}",
    defaults: new { controller = "Places", action = "Details" });

app.MapControllerRoute(
    name: "calendar_takvim",
    pattern: "takvim",
    defaults: new { controller = "Calendar", action = "Index" });

app.MapControllerRoute(
    name: "events_calendar",
    pattern: "etkinlik-takvimi",
    defaults: new { controller = "Events", action = "Calendar" });

app.MapControllerRoute(
    name: "events_index",
    pattern: "etkinlikler",
    defaults: new { controller = "Events", action = "Index" });

app.MapControllerRoute(
    name: "event_details",
    pattern: "etkinlik-detay/{id}",
    defaults: new { controller = "Events", action = "Detail" });

app.MapControllerRoute(
    name: "favorite_toggle",
    pattern: "favori-toggle/{placeId}",
    defaults: new { controller = "Account", action = "ToggleFavorite" });

app.MapControllerRoute(
    name: "routes_index",
    pattern: "rotalar",
    defaults: new { controller = "Routes", action = "Index" });

app.MapControllerRoute(
    name: "route_detail",
    pattern: "rota-detay/{id}",
    defaults: new { controller = "Routes", action = "Detail" });

app.MapControllerRoute(
    name: "user_login",
    pattern: "giris-yap",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "user_register",
    pattern: "kayit-ol",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "user_logout",
    pattern: "cikis-yap",
    defaults: new { controller = "Account", action = "Logout" });

app.MapControllerRoute(
    name: "user_profile",
    pattern: "profilim",
    defaults: new { controller = "Account", action = "Profile" });

app.MapControllerRoute(
    name: "home_about",
    pattern: "hakkimizda",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "home_contact",
    pattern: "iletisim",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "home_routes",
    pattern: "rotalar",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "home_explore",
    pattern: "kesfet",
    defaults: new { controller = "Home", action = "Index" });

// 3. Fallback Route Yapısı (Geriye Uyumluluk)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Role & Initial Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Admin Kullanıcısı Oluşturma
        var adminEmail = "admin@tourist.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = "Admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Mevcut kullanıcıyı da Admin rolüne dahil et
        var seherUser = await userManager.FindByEmailAsync("seherplt@gmail.com");
        if (seherUser != null && !await userManager.IsInRoleAsync(seherUser, "Admin"))
        {
            await userManager.AddToRoleAsync(seherUser, "Admin");
        }

        // Örnek Gezi Rotaları & Duraklar Seed İşlemi
        await TourismGuide.Web.Data.DbInitializer.SeedRoutesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Roller ve Admin oluşturulurken bir hata meydana geldi.");
    }
}

app.Run();