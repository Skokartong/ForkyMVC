using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Restaurant
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int AmountOfSeats { get; set; }

        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
    }
}
