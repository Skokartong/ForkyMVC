using ForkyMVC.Models.Account;
using ForkyMVC.Models.Booking;
using ForkyMVC.Models.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
using System.Text;

namespace RestaurantMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _client;
        private readonly string baseUri = "https://localhost:7275/";

        public AdminController(HttpClient client)
        {
            _client = client;
        }

        private void SetAuthorizationHeader()
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        // VIEWING ACTIONS
        public async Task<IActionResult> ViewAccounts()
        {
            var response = await _client.GetAsync($"{baseUri}viewaccounts");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<AccountViewModel>>(json);
                return View(customers);
            }
            TempData["ErrorMessage"] = "Unable to retrieve accounts. Please try again.";
            return View("Error");
        }

        public async Task<IActionResult> ViewRestaurants()
        {
            var response = await _client.GetAsync($"{baseUri}viewrestaurants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var restaurants = JsonConvert.DeserializeObject<List<RestaurantViewModel>>(json);
                ViewBag.NewRestaurant = new RestaurantViewModel();
                return View("ViewRestaurants", restaurants);
            }
            TempData["ErrorMessage"] = "Unable to retrieve restaurants. Please try again.";
            return View("Error");
        }

        public async Task<IActionResult> ViewMenus()
        {
            var response = await _client.GetAsync($"{baseUri}viewmenus");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var menus = JsonConvert.DeserializeObject<List<MenuViewModel>>(json);
                ViewBag.NewMenu = new MenuViewModel();
                return View("ViewMenus", menus);
            }
            TempData["ErrorMessage"] = "Unable to retrieve menus. Please try again.";
            return View("Error");
        }

        public async Task<IActionResult> ViewBookings()
        {
            var response = await _client.GetAsync($"{baseUri}viewbookings");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<List<ViewBookingViewModel>>(json);
                return View(bookings);
            }
            TempData["ErrorMessage"] = "Unable to retrieve bookings. Please try again.";
            return View("Error");
        }

        public async Task<IActionResult> ViewTables()
        {
            var response = await _client.GetAsync($"{baseUri}viewtables");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tables = JsonConvert.DeserializeObject<List<TableViewModel>>(json);
                ViewBag.NewTable = new TableViewModel();
                return View("ViewTables", tables);
            }
            TempData["ErrorMessage"] = "Unable to retrieve tables. Please try again.";
            return View("Error");
        }

        // ADD actions
        [HttpPost]
        public async Task<IActionResult> AddRestaurant(RestaurantViewModel restaurantViewModel)
        {
            var json = JsonConvert.SerializeObject(restaurantViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addrestaurant", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Restaurant added successfully!";
                return RedirectToAction("ViewRestaurants");
            }

            TempData["ErrorMessage"] = "Error adding restaurant. Please try again.";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> AddTable(NewTableViewModel tableViewModel)
        {
            var json = JsonConvert.SerializeObject(tableViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addtable", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Table added successfully!";
                return RedirectToAction("ViewTables");
            }

            TempData["ErrorMessage"] = "Error adding table. Please try again.";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> AddMenuItem(NewDishViewModel dishViewModel)
        {
            var json = JsonConvert.SerializeObject(dishViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addmenuitem", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Menu item added successfully!";
                return RedirectToAction("ViewMenus");
            }

            TempData["ErrorMessage"] = "Error adding menu item. Please try again.";
            return View("Error");
        }

        // UPDATE ACTIONS
        public async Task<IActionResult> UpdateRestaurant(int restaurantId)
        {
            var response = await _client.GetAsync($"{baseUri}viewrestaurant/{restaurantId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve restaurant details. Please try again.";
                return View("Error");
            }

            var json = await response.Content.ReadAsStringAsync();
            var restaurant = JsonConvert.DeserializeObject<RestaurantViewModel>(json);
            return View(restaurant);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRestaurant(RestaurantViewModel restaurantViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(restaurantViewModel);
            }

            var json = JsonConvert.SerializeObject(restaurantViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUri}updaterestaurant/{restaurantViewModel.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Restaurant updated successfully!";
                return RedirectToAction("ViewRestaurants");
            }

            TempData["ErrorMessage"] = "Error updating restaurant. Please try again.";
            return View("Error");
        }

        public async Task<IActionResult> UpdateMenuItem(int menuId)
        {
            var response = await _client.GetAsync($"{baseUri}viewmenuitem/{menuId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var menuItem = JsonConvert.DeserializeObject<MenuViewModel>(json);
                return View(menuItem);
            }

            TempData["ErrorMessage"] = "Unable to retrieve menu item details. Please try again.";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMenuItem(int menuId, MenuViewModel menuViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(menuViewModel);
            }

            var json = JsonConvert.SerializeObject(menuViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUri}updatemenuitem/{menuId}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Menu item updated successfully!";
                return RedirectToAction("ViewMenus", new { restaurantId = menuViewModel.FK_RestaurantId });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error updating menu item: {errorMessage}");
            return View(menuViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTable(TableViewModel tableViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(tableViewModel);
            }

            var json = JsonConvert.SerializeObject(tableViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUri}updatetable/{tableViewModel.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Table updated successfully!";
                return RedirectToAction("ViewTables", new { restaurantId = tableViewModel.FK_RestaurantId });
            }

            TempData["ErrorMessage"] = "Error updating table. Please try again.";
            return View("Error");
        }

        // DELETE ACTIONS
        [HttpPost]
        public async Task<IActionResult> DeleteRestaurant(int restaurantId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deleterestaurant/{restaurantId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Restaurant deleted successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Error deleting restaurant. Please try again.";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTable(int tableId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deletetable/{tableId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Table deleted successfully!";
                var tableResponse = await _client.GetAsync($"{baseUri}viewtable/{tableId}");
                if (tableResponse.IsSuccessStatusCode)
                {
                    var json = await tableResponse.Content.ReadAsStringAsync();
                    var table = JsonConvert.DeserializeObject<TableViewModel>(json);
                    return RedirectToAction("ViewTables", new { restaurantId = table?.FK_RestaurantId });
                }
                TempData["ErrorMessage"] = "Error retrieving table details after deletion.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting table. Please try again.";
            }
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDish(int menuId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deletedish/{menuId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Dish deleted successfully!";
                return RedirectToAction("ViewMenus", new { restaurantId = ViewBag.RestaurantId });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error deleting menu item: {errorMessage}");
            TempData["ErrorMessage"] = "Error deleting dish. Please try again.";
            return View("Error");
        }
    }
}
