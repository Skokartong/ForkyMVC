using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Restaurant.Add
{
    public class AddTableViewModel
    {
        public int TableNumber { get; set; }
        public int AmountOfSeats { get; set; }

        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
    }
}
