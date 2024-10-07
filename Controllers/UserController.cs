﻿using ForkyMVC.Models.Account;
using ForkyMVC.Models.Booking;
using ForkyMVC.Models.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace RestaurantMVC.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class UserController : Controller
    {
        private readonly HttpClient _client;
        private string baseUri = "https://localhost:7275/";

        public UserController(HttpClient client)
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

        // BOOKING AND RESTAURANTS ACTÌONS
        public async Task<IActionResult> ViewRestaurants()
        {
            var response = await _client.GetAsync($"{baseUri}viewrestaurants");
            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var restaurants = JsonConvert.DeserializeObject<List<RestaurantViewModel>>(jsonResponse);

            return View(restaurants);
        }

        public async Task<IActionResult> ViewMenu(int restaurantId)
        {
            var response = await _client.GetAsync($"{baseUri}viewmenu/{restaurantId}");
            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var menu = JsonConvert.DeserializeObject<List<MenuViewModel>>(jsonResponse);

            return View(menu);
        }

        public async Task<IActionResult> ViewBookings()
        {
            var userClaims = HttpContext.User;

            var userIdClaim = userClaims.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var response = await _client.GetAsync($"{baseUri}viewbookings/{userIdClaim.Value}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var bookings = JsonConvert.DeserializeObject<List<ViewBookingViewModel>>(jsonResponse);
                return View(bookings);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = "Could not find any bookings: " + errorMessage;
            return View("Error");
        }

        public async Task<IActionResult> BookTable(int restaurantId)
        {
            var userClaims = HttpContext.User;

            var userIdClaim = userClaims.Claims.FirstOrDefault(c => c.Type == "nameid");
            
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (int.TryParse(userIdClaim.Value, out int accountId))
            {
                var viewModel = new AddBookingViewModel
                {
                    FK_RestaurantId = restaurantId,
                    FK_AccountId = accountId
                };

                return View(viewModel);
            }

            return View("Error");
        }


        [HttpPost]
        public async Task<IActionResult> BookTable(AddBookingViewModel addBookingViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(addBookingViewModel);
            }

            var availabilityCheck = new AvailabilityCheckViewModel
            {
                FK_RestaurantId = addBookingViewModel.FK_RestaurantId,
                StartTime = addBookingViewModel.BookingStart,
                EndTime = addBookingViewModel.BookingEnd,
                NumberOfGuests = addBookingViewModel.NumberOfGuests,
            };

            var availabilityResponse = await _client.PostAsJsonAsync($"{baseUri}checkavailability", availabilityCheck);

            Console.WriteLine(availabilityResponse.Content);

            if (availabilityResponse.IsSuccessStatusCode)
            {
                var availableTables = await availabilityResponse.Content.ReadAsAsync<List<TableViewModel>>();

                if (availableTables.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "No available tables for the selected time and number of guests.");
                    return View(addBookingViewModel);
                }
            }
            else
            {
                var error = await availabilityResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, "Could not check availability: " + error);
                return View(addBookingViewModel);
            }

            var json = JsonConvert.SerializeObject(addBookingViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await _client.PostAsync($"{baseUri}newbooking", content);

            Console.WriteLine($"response: {response}");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.SuccessMessage = "Table booked successfully!";
                return RedirectToAction("ViewBookings");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = "Could not book table: " + errorMessage;
            return View(addBookingViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int bookingId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deletebooking/{bookingId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Booking deleted successfully!";
                return RedirectToAction("ViewBookings");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = "Could not delete booking: " + errorMessage;
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBooking(int bookingId, UpdateBookingViewModel updateBookingViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateBookingViewModel);
            }

            var json = JsonConvert.SerializeObject(updateBookingViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUri}updatebooking/{bookingId}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Booking updated successfully!";
                return RedirectToAction("ViewBookings");
            }

            return View("Error");
        }
    }
}
