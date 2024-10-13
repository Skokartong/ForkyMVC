namespace ForkyMVC.Models.Restaurant.View
{
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string TypeOfRestaurant { get; set; }
        public string Location { get; set; }
        public string? AdditionalInformation { get; set; }
        public List<MenuViewModel> Menus { get; set; }
    }
}
