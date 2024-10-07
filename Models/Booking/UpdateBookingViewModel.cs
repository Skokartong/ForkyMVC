using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Booking
{
    public class UpdateBookingViewModel
    {
        public int NumberOfGuests { get; set; }
        public DateTime BookingStart { get; set; }
        public DateTime BookingEnd { get; set; }
        public string? Message { get; set; }
        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
    }
}
