using JwtDayMusic.WebApi.Dtos;

namespace JwtDayMusic.WebApi.Services.RegisterServices
{
    public interface IRegisterService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
    }
}
