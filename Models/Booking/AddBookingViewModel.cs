using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Booking
{
    public class AddBookingViewModel
    {
        public int NumberOfGuests { get; set; }
        public DateTime BookingStart { get; set; }
        public DateTime BookingEnd { get; set; }
        public string? Message { get; set; }
        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
        [ForeignKey("Account")]
        public int FK_AccountId { get; set; }
    }
}
