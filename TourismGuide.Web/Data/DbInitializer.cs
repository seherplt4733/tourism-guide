using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAndRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            // 1. "Admin" ve "User" Rollerini Kontrol Et ve Oluştur
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("[TOURIST AUTH] '{Role}' rolü oluşturuldu.", role);
                }
            }

            // 2. Tek Ana Yönetici (Admin) Hesabını Tanımla
            const string adminEmail = "admin@tourist.com";
            const string adminPass = "Admin123*";
            const string adminFullName = "Sistem Yöneticisi";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminFullName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPass);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("[TOURIST AUTH] Ana yönetici hesabı oluşturuldu.");
                }
                else
                {
                    logger.LogError("[TOURIST AUTH] Yönetici hesabı oluşturulamadı: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Bilgilerini ve şifresini garanti altına al
                adminUser.UserName = adminFullName;
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);

                if (!await userManager.CheckPasswordAsync(adminUser, adminPass))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    await userManager.ResetPasswordAsync(adminUser, token, adminPass);
                    logger.LogInformation("[TOURIST AUTH] Yönetici şifresi güncellendi: {Pass}", adminPass);
                }

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. YALNIZCA TEK BİR ADMİN KURALI:
            // admin@tourist.com dışındaki diğer tüm hesaplardan "Admin" rolünü kaldır, yalnızca "User" rolü ver
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var userRole = await roleManager.FindByNameAsync("User");

            if (adminRole != null && userRole != null)
            {
                // 3.1. Ana yöneticinin sadece Admin rolü olmasını sağla
                await context.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = {0} AND RoleId = {1}) " +
                    "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ({0}, {1}); " +
                    "DELETE FROM AspNetUserRoles WHERE UserId = {0} AND RoleId = {2};",
                    adminUser.Id, adminRole.Id, userRole.Id);

                // 3.2. Ana yönetici dışındaki kullanıcılardan Admin rolünü kaldır
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AspNetUserRoles WHERE RoleId = {0} AND UserId <> {1};",
                    adminRole.Id, adminUser.Id);

                // 3.3. Diğer tüm kullanıcılarda eksikse User rolünü tanımla
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO AspNetUserRoles (UserId, RoleId) " +
                    "SELECT u.Id, {0} FROM AspNetUsers u " +
                    "WHERE u.Id <> {1} AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = {0});",
                    userRole.Id, adminUser.Id);

                logger.LogInformation("[TOURIST AUTH] Yalnızca tek admin kuralı başarıyla uygulandı.");
            }

            // 4. Konsol & Terminal Çıktısı
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine("[TOURIST ADMIN] Yönetici Hesabı Hazır -> E-posta: admin@tourist.com | Şifre: Admin123*");
            Console.WriteLine("===============================================================================");
            Console.WriteLine();
            Console.ResetColor();

            logger.LogInformation("[TOURIST ADMIN] Yönetici Hesabı Hazır -> E-posta: {Email} | Şifre: {Pass}", adminEmail, adminPass);
        }

        public static async Task SeedRoutesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            // Kategori kontrolü (Mekanlar için)
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Tarih") || c.Name.Contains("Kültür"))
                ?? await context.Categories.FirstOrDefaultAsync();
            if (category == null)
            {
                category = new Category
                {
                    Name = "Tarihi ve Kültürel Yerler",
                    IconCssClass = "fa fa-landmark"
                };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            // 1. Şehirleri Kontrol Et ve Oluştur
            var istanbul = await context.Cities.FirstOrDefaultAsync(c => c.Name == "İstanbul" || c.Name == "Istanbul");
            if (istanbul == null)
            {
                istanbul = new City
                {
                    Name = "İstanbul",
                    Description = "Tarihi ve kültürel dokusuyla Doğu ile Batı'nın buluştuğu eşsiz metropol.",
                    ImageUrl = "/img/destination-1.jpg"
                };
                context.Cities.Add(istanbul);
                await context.SaveChangesAsync();
            }

            var izmir = await context.Cities.FirstOrDefaultAsync(c => c.Name == "İzmir" || c.Name == "Izmir");
            if (izmir == null)
            {
                izmir = new City
                {
                    Name = "İzmir",
                    Description = "Ege'nin incisi, kordonu ve tarihi zenginlikleriyle ünlü sahil kenti.",
                    ImageUrl = "/img/destination-2.jpg"
                };
                context.Cities.Add(izmir);
                await context.SaveChangesAsync();
            }

            var nevsehir = await context.Cities.FirstOrDefaultAsync(c => c.Name.Contains("Nevşehir") || c.Name.Contains("Nevsehir") || c.Name.Contains("Kapadokya"));
            if (nevsehir == null)
            {
                nevsehir = new City
                {
                    Name = "Nevşehir (Kapadokya)",
                    Description = "Masalsı peribacaları, tarihi vadileri ve eşsiz yer altı şehirleriyle büyüleyici bir açık hava müzesi.",
                    ImageUrl = "/img/destination-3.jpg"
                };
                context.Cities.Add(nevsehir);
                await context.SaveChangesAsync();
            }

            // 2. Mekanları Kontrol Et ve Oluştur (Duraklarla koordinat ve görsel eşleşmesi için)
            async Task<Place> EnsurePlaceAsync(string name, string shortDesc, string address, decimal price, double lat, double lng, int cityId, string imgUrl)
            {
                var existing = await context.Places.FirstOrDefaultAsync(p => p.CityId == cityId && p.Title == name);
                if (existing != null) return existing;

                var place = new Place
                {
                    Title = name,
                    ShortDescription = shortDesc,
                    FullDescription = shortDesc,
                    Address = address,
                    Price = price,
                    Latitude = lat,
                    Longitude = lng,
                    Rating = 4.8,
                    ImageUrl = imgUrl,
                    CityId = cityId,
                    CategoryId = category.Id
                };
                context.Places.Add(place);
                await context.SaveChangesAsync();
                return place;
            }

            // İstanbul mekanları
            var pAyasofya = await EnsurePlaceAsync("Ayasofya-i Kebir Cami-i Şerifi", "Bizans ve Osmanlı dönemlerinin simge yapısı, dünya mimarlık tarihinin başyapıtlarından biri.", "Sultanahmet Meydanı, Fatih / İstanbul", 0, 41.0086, 28.9802, istanbul.Id, "/img/destination-1.jpg");
            var pTopkapi = await EnsurePlaceAsync("Topkapı Sarayı", "Osmanlı padişahlarının yüzyıllar boyu devleti yönettiği görkemli saray kompleksi.", "Cankurtaran, Fatih / İstanbul", 500, 41.0115, 28.9834, istanbul.Id, "/img/destination-4.jpg");
            var pKapalicarsi = await EnsurePlaceAsync("Kapalıçarşı & Mısır Çarşısı", "Dünyanın en eski ve en büyük kapalı çarşılarından biri, binlerce dükkan ve otantik lezzetler.", "Beyazıt, Fatih / İstanbul", 0, 41.0108, 28.9680, istanbul.Id, "/img/destination-1.jpg");

            // İzmir mekanları
            var pKordon = await EnsurePlaceAsync("Alsancak Kordon & Pasaport İskelesi", "İzmir'in simgesi, masmavi körfez manzarası ve boyoz kahvaltısıyla ünlü sahil bandı.", "Alsancak, Konak / İzmir", 0, 38.4333, 27.1390, izmir.Id, "/img/destination-2.jpg");
            var pSaatKulesi = await EnsurePlaceAsync("İzmir Saat Kulesi & Konak Meydanı", "1901 yılında inşa edilen, Konak Meydanı'nı süsleyen İzmir'in en zarif simgesi.", "Konak Meydanı, Konak / İzmir", 0, 38.4189, 27.1287, izmir.Id, "/img/destination-2.jpg");
            var pKemeralti = await EnsurePlaceAsync("Tarihi Kemeraltı Çarşısı & Kızlarağası Hanı", "Tarihi Osmanlı hanları, közde Türk kahvesi ve zanaatkarlarıyla yaşayan devasa çarşı.", "Kemeraltı, Konak / İzmir", 0, 38.4195, 27.1325, izmir.Id, "/img/destination-2.jpg");

            // Nevşehir mekanları
            var pGoreme = await EnsurePlaceAsync("Göreme Açık Hava Müzesi", "Kaya içine oyulmuş kiliseleri, bin yıllık freskleri ve manastır yaşamıyla UNESCO Dünya Mirası.", "Göreme, Nevşehir", 400, 38.6431, 34.8289, nevsehir.Id, "/img/destination-3.jpg");
            var pPasabag = await EnsurePlaceAsync("Paşabağları (Rahipler Vadisi)", "Üç başlı peribacalarının en görkemli örneklerinin yer aldığı eşsiz Kapadokya vadisi.", "Zelve Yolu, Paşabağı, Nevşehir", 0, 38.6775, 34.8530, nevsehir.Id, "/img/destination-3.jpg");
            var pUchisar = await EnsurePlaceAsync("Uçhisar Kalesi", "Kapadokya'nın en yüksek noktası, tüm vadiye hakim panoramik gün batımı manzarası.", "Uçhisar, Nevşehir", 150, 38.6300, 34.8056, nevsehir.Id, "/img/destination-3.jpg");

            // 3. Rotaları Kontrol Et ve Ekle

            // ROTA 1: İstanbul - "Tarihi Yarımada Klasik Rota"
            var route1Title = "Tarihi Yarımada Klasik Rota";
            var r1 = await context.RoutePlans.Include(r => r.Stops).FirstOrDefaultAsync(r => r.Title == route1Title);
            if (r1 == null)
            {
                r1 = new RoutePlan
                {
                    Title = route1Title,
                    CityId = istanbul.Id,
                    CoverImageUrl = "/img/destination-1.jpg",
                    EstimatedDuration = "6-7 Saat",
                    TotalDistance = "4.5 km",
                    Summary = "İstanbul'un kalbinde Ayasofya'dan başlayıp Kapalıçarşı'nın otantik sokaklarına uzanan tarih dolu bir gün.",
                    IsActive = true,
                    Stops = new List<RouteStop>
                    {
                        new RouteStop
                        {
                            StopOrder = 1,
                            CustomTitle = "Ayasofya-i Kebir Cami-i Şerifi",
                            SuggestedTime = "1.5 Saat",
                            Note = "Sabah erken saatte ziyaret etmek kuyrukları önler.",
                            TransportationTip = "Topkapı Sarayı'na doğru 5 dakikalık kısa bir yürüyüş.",
                            PlaceId = pAyasofya.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 2,
                            CustomTitle = "Topkapı Sarayı",
                            SuggestedTime = "2.5 Saat",
                            Note = "Harem dairesini mutlaka ayrı gezin.",
                            TransportationTip = "Gülhane Parkı üzerinden veya tramvay yolu boyunca Kapalıçarşı'ya yürüyüş (yaklaşık 15 dk).",
                            PlaceId = pTopkapi.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 3,
                            CustomTitle = "Kapalıçarşı & Mısır Çarşısı",
                            SuggestedTime = "2 Saat",
                            Note = "Çarşı içinde geleneksel Türk kahvesi molası verin.",
                            TransportationTip = null,
                            PlaceId = pKapalicarsi.Id
                        }
                    }
                };
                context.RoutePlans.Add(r1);
                logger.LogInformation("[TOURIST ROUTES] Rota 1 eklendi: {Title}", route1Title);
            }

            // ROTA 2: İzmir - "Kordon'dan Tarihi Kemeraltı'na İzmir Keşfi"
            var route2Title = "Kordon'dan Tarihi Kemeraltı'na İzmir Keşfi";
            var r2 = await context.RoutePlans.Include(r => r.Stops).FirstOrDefaultAsync(r => r.Title == route2Title);
            if (r2 == null)
            {
                r2 = new RoutePlan
                {
                    Title = route2Title,
                    CityId = izmir.Id,
                    CoverImageUrl = "/img/destination-2.jpg",
                    EstimatedDuration = "4-5 Saat",
                    TotalDistance = "3.8 km",
                    Summary = "İzmir Körfezi esintisiyle Alsancak Kordon'dan başlayıp Saat Kulesi ve tarihi Kemeraltı Çarşısı'na uzanan keyifli bir yürüyüş.",
                    IsActive = true,
                    Stops = new List<RouteStop>
                    {
                        new RouteStop
                        {
                            StopOrder = 1,
                            CustomTitle = "Alsancak Kordon & Pasaport İskelesi",
                            SuggestedTime = "1.5 Saat",
                            Note = "Sabah yürüyüşü ve boyoz kahvaltısı yapın.",
                            TransportationTip = "Kordon boyu Konak Meydanı ve Saat Kulesi'ne doğru yürüyüş (yaklaşık 20 dk).",
                            PlaceId = pKordon.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 2,
                            CustomTitle = "İzmir Saat Kulesi & Konak Meydanı",
                            SuggestedTime = "45 Dk",
                            Note = "Güvercinleri besleyip meydan fotoğrafları çekin.",
                            TransportationTip = "Meydandan tarihi çarşı girişine doğru 3-5 dakikalık geçiş.",
                            PlaceId = pSaatKulesi.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 3,
                            CustomTitle = "Tarihi Kemeraltı Çarşısı & Kızlarağası Hanı",
                            SuggestedTime = "2 Saat",
                            Note = "Fincanda pişen meşhur közde kahveyi için.",
                            TransportationTip = null,
                            PlaceId = pKemeralti.Id
                        }
                    }
                };
                context.RoutePlans.Add(r2);
                logger.LogInformation("[TOURIST ROUTES] Rota 2 eklendi: {Title}", route2Title);
            }

            // ROTA 3: Nevşehir - "Kapadokya Vadileri ve Masalsı Peribacaları"
            var route3Title = "Kapadokya Vadileri ve Masalsı Peribacaları";
            var r3 = await context.RoutePlans.Include(r => r.Stops).FirstOrDefaultAsync(r => r.Title == route3Title);
            if (r3 == null)
            {
                r3 = new RoutePlan
                {
                    Title = route3Title,
                    CityId = nevsehir.Id,
                    CoverImageUrl = "/img/destination-3.jpg",
                    EstimatedDuration = "5-6 Saat",
                    TotalDistance = "8.0 km",
                    Summary = "Göreme açık hava müzesinden başlayıp Paşabağları ve Uçhisar Kalesi ile sonlanan masalsı Kapadokya rotası.",
                    IsActive = true,
                    Stops = new List<RouteStop>
                    {
                        new RouteStop
                        {
                            StopOrder = 1,
                            CustomTitle = "Göreme Açık Hava Müzesi",
                            SuggestedTime = "2 Saat",
                            Note = "Karanlık Kilise'nin korunmuş fresklerini görün.",
                            TransportationTip = "Araç veya tur servisi ile Paşabağları Vadisi'ne geçiş (yaklaşık 10 dk).",
                            PlaceId = pGoreme.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 2,
                            CustomTitle = "Paşabağları (Rahipler Vadisi)",
                            SuggestedTime = "1.5 Saat",
                            Note = "Üç başlı peribacalarının arasında fotoğraf çekin.",
                            TransportationTip = "Uçhisar Kalesi yönüne araçla 15 dakikalık güzergah.",
                            PlaceId = pPasabag.Id
                        },
                        new RouteStop
                        {
                            StopOrder = 3,
                            CustomTitle = "Uçhisar Kalesi",
                            SuggestedTime = "1.5 Saat",
                            Note = "Gün batımını izlemek için kalenin en tepe noktasına çıkın.",
                            TransportationTip = null,
                            PlaceId = pUchisar.Id
                        }
                    }
                };
                context.RoutePlans.Add(r3);
                logger.LogInformation("[TOURIST ROUTES] Rota 3 eklendi: {Title}", route3Title);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("[TOURIST ROUTES] 3 yeni detaylı rota başarıyla kontrol edildi / eklendi.");
        }
    }
}
