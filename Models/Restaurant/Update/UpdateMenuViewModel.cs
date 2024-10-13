using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForkyMVC.Models.Restaurant.Update
{
    public class UpdateMenuViewModel
    {
        public int Id { get; set; }
        public string NameOfDish { get; set; }
        public string Drink { get; set; }
        public bool IsAvailable { get; set; }
        public string? Ingredients { get; set; }
        public double Price { get; set; }
        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
    }
}
