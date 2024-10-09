using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RestaurantMVC.Controllers
{
    // Controller that is available to anyone regardless of authorization level
    // This controller is meant for displaying basic info about Forky

    public class HomeController : Controller
    {
        private readonly HttpClient _client;
        private string baseUri = "https://localhost:7275/";

        public HomeController(HttpClient client)
        {
            _client = client;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Our Booking System";

            return View();
        }
    }
}

