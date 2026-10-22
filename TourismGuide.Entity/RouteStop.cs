using System.ComponentModel.DataAnnotations;

namespace TourismGuide.Entity
{
    public class RouteStop
    {
        public int Id { get; set; }

        public int RoutePlanId { get; set; }
        public virtual RoutePlan? RoutePlan { get; set; }

        public int StopOrder { get; set; }

        public int? PlaceId { get; set; }
        public virtual Place? Place { get; set; }

        [StringLength(200)]
        public string? CustomTitle { get; set; }

        [StringLength(100)]
        public string? SuggestedTime { get; set; }

        public string? TransportationTip { get; set; }

        public string? Note { get; set; }
    }
}
