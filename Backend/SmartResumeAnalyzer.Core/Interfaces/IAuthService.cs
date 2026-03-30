using SmartResumeAnalyzer.Core.DTOs.Auth;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
