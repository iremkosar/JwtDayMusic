using JwtDayMusic.WebApi.Dtos;

namespace JwtDayMusic.WebApi.Services.LoginServices
{
    public interface ILoginService
    {
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
