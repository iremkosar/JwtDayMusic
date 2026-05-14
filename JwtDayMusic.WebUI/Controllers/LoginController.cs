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
            var jsonData=JsonConvert.SerializeObject(loginDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7180/api/Login", stringContent);          

            var responseJson=await responseMessage.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<ResponseTokenDto>(responseJson);
            string token = tokenResponse.Token;

            HttpContext.Session.SetString("JwtToken", token);

            return View();
        }

    }
}
