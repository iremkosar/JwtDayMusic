using JwtDayMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace JwtDayMusic.WebUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(LoginDto loginDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(loginDto);
            var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7180/api/Login", stringContent);

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<ResponseTokenDto>(responseJson);

            // Token yoksa ya da API "hata" stringi dönmüşse, session'a hiç yazmadan geri dön
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token) || tokenResponse.Token == "hata")
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(loginDto);
            }

            HttpContext.Session.SetString("JwtToken", tokenResponse.Token);
            return RedirectToAction("Index", "Song");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SignIn", "Login");
        }

    }
}
