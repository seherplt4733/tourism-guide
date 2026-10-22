using System.Globalization;

namespace TourismGuide.Business.Utilities
{
    public static class TextHelper
    {
        private static readonly CultureInfo TrCulture = new CultureInfo("tr-TR");

        /// <summary>
        /// Metni Türkçe kurallarına uygun olarak Title Case (Her Kelimenin İlk Harfi Büyük) formatına çevirir.
        /// Örn: 'ankara' -> 'Ankara', 'kuğulu park' -> 'Kuğulu Park'
        /// </summary>
        public static string ToTitleCaseTurkish(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var trimmed = text.Trim();
            return TrCulture.TextInfo.ToTitleCase(trimmed.ToLower(TrCulture));
        }

        /// <summary>
        /// İki metni büyük/küçük harf duyarsız (OrdinalIgnoreCase) ve kırpılmış olarak karşılaştırır.
        /// </summary>
        public static bool EqualsIgnoreCase(string? a, string? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<string, (string Plate, string Abbr)> CityPlates = new()
        {
            { "adana", ("01", "ADA") },
            { "adıyaman", ("02", "ADI") },
            { "afyon", ("03", "AFY") },
            { "ağrı", ("04", "AĞR") },
            { "amasya", ("05", "AMA") },
            { "ankara", ("06", "ANK") },
            { "antalya", ("07", "ANT") },
            { "artvin", ("08", "ART") },
            { "aydın", ("09", "AYD") },
            { "balıkesir", ("10", "BAL") },
            { "bilecik", ("11", "BİL") },
            { "bingöl", ("12", "BİN") },
            { "bitlis", ("13", "BİT") },
            { "bolu", ("14", "BOL") },
            { "burdur", ("15", "BUR") },
            { "bursa", ("16", "BRS") },
            { "çanakkale", ("17", "ÇAN") },
            { "çankırı", ("18", "ÇAN") },
            { "çorum", ("19", "ÇOR") },
            { "denizli", ("20", "DEN") },
            { "diyarbakır", ("21", "DİY") },
            { "edirne", ("22", "EDİ") },
            { "elazığ", ("23", "ELA") },
            { "erzincan", ("24", "ERZ") },
            { "erzurum", ("25", "ERZ") },
            { "eskişehir", ("26", "ESK") },
            { "gaziantep", ("27", "GAZ") },
            { "giresun", ("28", "GİR") },
            { "gümüşhane", ("29", "GÜM") },
            { "hakkari", ("30", "HAK") },
            { "hatay", ("31", "HAT") },
            { "ısparta", ("32", "ISP") },
            { "mersin", ("33", "MER") },
            { "içel", ("33", "MER") },
            { "istanbul", ("34", "İST") },
            { "i̇stanbul", ("34", "İST") },
            { "izmir", ("35", "İZM") },
            { "i̇zmir", ("35", "İZM") },
            { "kars", ("36", "KAR") },
            { "kastamonu", ("37", "KAS") },
            { "kayseri", ("38", "KAY") },
            { "kırklareli", ("39", "KIR") },
            { "kırşehir", ("40", "KIR") },
            { "kocaeli", ("41", "KOC") },
            { "konya", ("42", "KON") },
            { "kütahya", ("43", "KÜT") },
            { "malatya", ("44", "MAL") },
            { "manisa", ("45", "MAN") },
            { "kahramanmaraş", ("46", "MAR") },
            { "mardin", ("47", "MAR") },
            { "muğla", ("48", "MUĞ") },
            { "muş", ("49", "MUŞ") },
            { "nevşehir", ("50", "NEV") },
            { "niğde", ("51", "NİĞ") },
            { "ordu", ("52", "ORD") },
            { "rize", ("53", "RİZ") },
            { "sakarya", ("54", "SAK") },
            { "samsun", ("55", "SAM") },
            { "siirt", ("56", "Sİİ") },
            { "sinop", ("57", "SİN") },
            { "sivas", ("58", "SİV") },
            { "tekirdağ", ("59", "TEK") },
            { "tokat", ("60", "TOK") },
            { "trabzon", ("61", "TRA") },
            { "tunceli", ("62", "TUN") },
            { "şanlıurfa", ("63", "URF") },
            { "uşak", ("64", "UŞA") },
            { "van", ("65", "VAN") },
            { "yozgat", ("66", "YOZ") },
            { "zonguldak", ("67", "ZON") },
            { "aksaray", ("68", "AKS") },
            { "bayburt", ("69", "BAY") },
            { "karaman", ("70", "KAR") },
            { "kırıkkale", ("71", "KRK") },
            { "batman", ("72", "BAT") },
            { "şırnak", ("73", "ŞIR") },
            { "bartın", ("74", "BAR") },
            { "ardahan", ("75", "ARD") },
            { "ığdır", ("76", "IĞD") },
            { "yalova", ("77", "YAL") },
            { "karabük", ("78", "KRB") },
            { "kilis", ("79", "KİL") },
            { "osmaniye", ("80", "OSM") },
            { "düzce", ("81", "DÜZ") }
        };

        /// <summary>
        /// Şehir adı için 3 harfli büyük kısaltma üretir (Örn: Ankara için 'ANK', İstanbul için 'İST', Nevşehir için 'NEV').
        /// Parantezli isimlerde (örn: 'Nevşehir (Kapadokya)') parantez öncesini baz alır.
        /// </summary>
        public static string GetCityAbbreviation(string? cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName)) return "ŞHR";

            var clean = cityName.Trim();
            // Parantezli isimlerde parantez öncesini baz al
            if (clean.Contains('('))
            {
                clean = clean.Split('(')[0].Trim();
            }

            // Türkçe karakter uyumlu yalnızca harfleri al
            var lettersOnly = new string(clean.Where(char.IsLetter).ToArray());
            if (lettersOnly.Length >= 3)
            {
                return lettersOnly.Substring(0, 3).ToUpper(TrCulture);
            }

            return clean.ToUpper(TrCulture);
        }

        /// <summary>
        /// Şehir rozet metnini döner (3 harfli kısaltma).
        /// </summary>
        public static string GetCityPlateBadge(string? cityName)
        {
            return GetCityAbbreviation(cityName);
        }
    }
}
