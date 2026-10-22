using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismGuide.Entity
{
    public class RoutePlan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rota başlığı zorunludur.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şehir seçimi zorunludur.")]
        public int CityId { get; set; }
        public virtual City? City { get; set; }

        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Tahmini süre zorunludur.")]
        [StringLength(100)]
        public string EstimatedDuration { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TotalDistance { get; set; }

        [Required(ErrorMessage = "Rota özeti zorunludur.")]
        public string Summary { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public virtual List<RouteStop> Stops { get; set; } = new();
    }
}
