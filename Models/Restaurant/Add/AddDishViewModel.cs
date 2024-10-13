using System.ComponentModel.DataAnnotations.Schema;

namespace ForkyMVC.Models.Restaurant.Add
{
    public class AddDishViewModel
    {
        public string NameOfDish { get; set; }
        public string Drink { get; set; }
        public bool IsAvailable { get; set; }
        public string? Ingredients { get; set; }
        public double Price { get; set; }
        [ForeignKey("Restaurant")]
        public int FK_RestaurantId { get; set; }
    }
}
