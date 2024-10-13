using System.ComponentModel.DataAnnotations;

namespace ForkyMVC.Models.Restaurant.Update
{
    public class UpdateRestaurantViewModel
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string TypeOfRestaurant { get; set; }
        public string Location { get; set; }
        public string? AdditionalInformation { get; set; }
    }
}
