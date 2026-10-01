using JwtDayMusic.WebApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtDayMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            IdentityRole identityRole = new IdentityRole
            {
                Name = roleName
            };
            await _roleManager.CreateAsync(identityRole);
            return Ok("Başarılı");
        }

        // Kullanıcı adı yazarak rol atama - SQL'le elle ID eşleştirmek yerine bu kullanılmalı.
        // Kullanıcının önceki tüm rollerini kaldırıp, yalnızca verilen rolü atar (üyelik "set" edilir).
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole(string userName, string roleName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound($"'{userName}' adlı kullanıcı bulunamadı.");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return NotFound($"'{roleName}' adlı rol bulunamadı.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"'{userName}' kullanıcısının üyeliği '{roleName}' olarak ayarlandı.");
        }

        // Bir kullanıcının o an hangi role sahip olduğunu görmek için (kontrol/teyit amaçlı)
        [HttpGet("of/{userName}")]
        public async Task<IActionResult> GetUserRoles(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound($"'{userName}' adlı kullanıcı bulunamadı.");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }
}
