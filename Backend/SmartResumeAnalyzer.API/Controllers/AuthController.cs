using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.Core.DTOs.Auth;
using SmartResumeAnalyzer.Core.Interfaces;
using System.Security.Claims;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.GetCurrentUserAsync(userId.Value);
            return Ok(profile);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _authService.UpdateProfileAsync(userId.Value, dto);
            return Ok(profile);
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _authService.ChangePasswordAsync(userId.Value, dto);
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return null;
            return userId;
        }
    }
}