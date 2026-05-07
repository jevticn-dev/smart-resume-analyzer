using SmartResumeAnalyzer.Core.DTOs.Auth;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserProfileDto> GetCurrentUserAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    }
}