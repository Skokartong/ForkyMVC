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
                return RedirectToAction("Login", new { message = "Account created successfully!" });
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
                throw new NotImplementedException();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            await Console.Out.WriteLineAsync($"jsonRes: {jsonResponse}");

            var token = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
            await Console.Out.WriteLineAsync($"token: {token}");

            if (token == null || string.IsNullOrEmpty(token.Token))
            {
                ModelState.AddModelError("", "No valid token");
                throw new NotImplementedException();
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Token);

            var claims = jwtToken.Claims.ToList();

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = jwtToken.ValidTo.AddHours(1),
            });

            HttpContext.Response.Cookies.Append("jwtToken", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = jwtToken.ValidTo,
                Path = "/"
            });

            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }

            var userRoleClaim = claims.FirstOrDefault(c => c.Type == "role");
            await Console.Out.WriteLineAsync($"user roleclaim: {userRoleClaim}");

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login");
            }

            var response = await _client.GetAsync($"{baseUri}getaccount/{userIdClaim.Value}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var accountViewModel = JsonConvert.DeserializeObject<AccountViewModel>(jsonResponse);
                return View(accountViewModel); 
            }

            return View("Error");
        }

        public async Task<IActionResult> UpdateAccount(int accountId)
        {
            var response = await _client.GetAsync($"{baseUri}getaccount/{accountId}");
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
                    UserName = account.UserName,
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
                return RedirectToAction("MyAccount", new { message = "Account updated successfully!" });
            }

            return View(updateAccountViewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var response = await _client.DeleteAsync($"{baseUri}deleteaccount/{accountId}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { message = "Account deleted successfully!" });
            }

            return RedirectToAction("Index", new { message = "Error deleting account." });
        }
    }
}