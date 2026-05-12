using JwtDayMusic.WebApi.Dtos;
using JwtDayMusic.WebApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtDayMusic.WebApi.Services.LoginServices
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {

            var user = await _userManager.FindByNameAsync(loginDto.Username);

            var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);

            if (result.Succeeded)
            {
                return await GenerateToken(user);
            }
            else
            {
                return "hata";
            }
        }

        private async Task<string> GenerateToken(AppUser appUser)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var roles = await _userManager.GetRolesAsync(appUser);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,appUser.Id),
                new Claim(ClaimTypes.Email,appUser.Email),
                new Claim(ClaimTypes.Name,appUser.Name),
                new Claim(ClaimTypes.Surname,appUser.Surname)
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims = claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
            
        }
    }
}
