using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Booking
{
    public class AvailabilityCheckViewModel
    {
        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
