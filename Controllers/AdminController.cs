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
    // This controller is specifically for admin.
    // Actions include adding restaurants, updating menus, viewing customer data e.g

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

        public async Task<IActionResult> ViewAccounts()
        {
            var response = await _client.GetAsync($"{baseUri}viewaccounts");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<AccountViewModel>>(json);
                return View(customers);
            }
            return View("Error");
        }

        public async Task<IActionResult> ViewAccount(int accountId)
        {
            var response = await _client.GetAsync($"{baseUri}viewaccount/{accountId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<AccountViewModel>(json);
                return View(customer);
            }

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
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> AddRestaurant(RestaurantViewModel restaurantViewModel)
        {
            var json = JsonConvert.SerializeObject(restaurantViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addrestaurant", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ViewRestaurants");
            }
            return View("Error");
        }

        public async Task<IActionResult> UpdateRestaurant(int restaurantId)
        {
            var response = await _client.GetAsync($"{baseUri}viewrestaurant/{restaurantId}");
            if (!response.IsSuccessStatusCode)
            {
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
                return RedirectToAction("ViewRestaurants");
            }
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRestaurant(int restaurantId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deleterestaurant/{restaurantId}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View("Error");
        }

        // Menu actions
        public async Task<IActionResult> ViewMenu(int restaurantId)
        {
            var response = await _client.GetAsync($"{baseUri}viewmenu/{restaurantId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var menuItems = JsonConvert.DeserializeObject<List<MenuViewModel>>(json);
                ViewBag.RestaurantId = restaurantId;
                return View(menuItems);
            }
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> AddMenuItem(MenuViewModel menuViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(menuViewModel);
            }

            var json = JsonConvert.SerializeObject(menuViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addmenuitem", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ViewMenu", new { restaurantId = menuViewModel.FK_RestaurantId });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error adding menu item: {errorMessage}");
            return View(menuViewModel);
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
                return RedirectToAction("ViewMenu", new { restaurantId = menuViewModel.FK_RestaurantId });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error updating menu item: {errorMessage}");
            return View(menuViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDish(int menuId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deletedish/{menuId}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ViewMenu");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error deleting menu item: {errorMessage}");
            return View("Error");
        }

        // Booking actions
        public async Task<IActionResult> ViewBookings()
        {
            var response = await _client.GetAsync($"{baseUri}viewbookings");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<List<ViewBookingViewModel>>(json);
                return View(bookings);
            }
            return View("Error");
        }

        public async Task<IActionResult> ViewTables(int restaurantId)
        {
            var response = await _client.GetAsync($"{baseUri}viewtables/{restaurantId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tables = JsonConvert.DeserializeObject<List<TableViewModel>>(json);
                ViewBag.RestaurantId = restaurantId;
                return View(tables);
            }
            return View("Error");
        }

        public IActionResult AddTable(int restaurantId)
        {
            ViewBag.RestaurantId = restaurantId; 
            return View(new TableViewModel()); 
        }

        [HttpPost]
        public async Task<IActionResult> AddTable(TableViewModel tableViewModel, int restaurantId)
        {
            tableViewModel.FK_RestaurantId = restaurantId; 

            var json = JsonConvert.SerializeObject(tableViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}addtable", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ViewTables", new { restaurantId = restaurantId }); 
            }
            return View("Error"); 
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
                return RedirectToAction("ViewTables", new { restaurantId = tableViewModel.FK_RestaurantId }); // Vid framgång, omdirigera
            }

            return View("Error"); 
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTable(int tableId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deletetable/{tableId}");
            if (response.IsSuccessStatusCode)
            {
                var tableResponse = await _client.GetAsync($"{baseUri}viewtable/{tableId}");
                if (tableResponse.IsSuccessStatusCode)
                {
                    var json = await tableResponse.Content.ReadAsStringAsync();
                    var table = JsonConvert.DeserializeObject<TableViewModel>(json);
                    return RedirectToAction("ViewTables", new { restaurantId = table?.FK_RestaurantId });
                }
                return View("Error");
            }
            return View("Error");
        }
    }
}

