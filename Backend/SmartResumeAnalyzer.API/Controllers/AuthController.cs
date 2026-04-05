using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.Core.DTOs.Auth;
using SmartResumeAnalyzer.Core.Interfaces;

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
    }
}
