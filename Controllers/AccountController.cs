using ForkyMVC.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantMVC.Controllers
{
    // Authentication and authorization controller
    // After user has been logged in, redirection to either Admin or Customer Controller happens

    public class AccountController : Controller
    {

        private readonly HttpClient _client;
        private readonly string baseUri = "https://localhost:7275/";

        public AccountController(HttpClient client)
        {
            _client = client;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var json = JsonConvert.SerializeObject(registerViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUri}register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account created successfully!";
                return RedirectToAction("Login");
            }

            return View(registerViewModel);
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginUser)
        {
            var response = await _client.PostAsJsonAsync($"{baseUri}login", loginUser);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Login failed: " + errorMessage);
                return View(loginUser);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);

            if (token == null || string.IsNullOrEmpty(token.Token))
            {
                ModelState.AddModelError("", "No valid token");
                return View(loginUser);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Token);

            var claims = jwtToken.Claims.ToList();

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = jwtToken.ValidTo.AddHours(1),
            });

            HttpContext.Response.Cookies.Append("jwtToken", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            var userRoleClaim = claims.FirstOrDefault(c => c.Type == "role");

            if (userRoleClaim != null)
            {
                if (userRoleClaim.Value == "Admin")
                {
                    Console.WriteLine("Admin login");
                    return RedirectToAction("Index", "Admin");
                }
                else if (userRoleClaim.Value == "User")
                {
                    Console.WriteLine("User login");
                    return RedirectToAction("Index", "User");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> MyAccount()
        {
            var userClaims = HttpContext.User;
            var accountId = userClaims.Claims.First(c => c.Type == "nameid").Value;

            var response = await _client.GetAsync($"{baseUri}getaccount/{accountId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var accountViewModel = JsonConvert.DeserializeObject<AccountViewModel>(jsonResponse);
                return View(accountViewModel);
            }

            return View("Error");
        }

        public async Task<IActionResult> UpdateAccount()
        {
            var userClaims = HttpContext.User;

            var userIdClaim = userClaims.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var response = await _client.GetAsync($"{baseUri}getaccount/{userIdClaim.Value}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var account = JsonConvert.DeserializeObject<AccountViewModel>(jsonResponse);

                var updateAccountViewModel = new UpdateAccountViewModel
                {
                    Id = account.Id,
                    Phone = account.Phone,
                    Address = account.Address,
                    Email = account.Email,
                    UserName = account.UserName
                };

                return View(updateAccountViewModel);
            }

            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccount(UpdateAccountViewModel updateAccountViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateAccountViewModel);
            }

            var json = JsonConvert.SerializeObject(updateAccountViewModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUri}updateaccount/{updateAccountViewModel.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("MyAccount");
            }

            return View(updateAccountViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var userClaims = HttpContext.User;

            var userIdClaim = userClaims.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var response = await _client.DeleteAsync($"{baseUri}deleteaccount/{userIdClaim.Value}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account deleted successfully!";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Error deleting account.";
            return RedirectToAction("Index", new { message = "Error deleting account." });

        }
    }
}