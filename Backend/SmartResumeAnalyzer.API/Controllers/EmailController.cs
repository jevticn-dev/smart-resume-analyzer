using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartResumeAnalyzer.API.Filters;
using SmartResumeAnalyzer.Core.DTOs.Email;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailApplicationService _emailApplicationService;

        public EmailController(IEmailApplicationService emailApplicationService)
        {
            _emailApplicationService = emailApplicationService;
        }

        [HttpPost("generate-draft")]
        [ServiceFilter(typeof(RateLimitFilter))]
        public async Task<IActionResult> GenerateDraft([FromBody] GenerateEmailDraftRequestDto dto)
        {
            var userId = GetUserId();
            var draft = await _emailApplicationService.GenerateDraftAsync(
                dto.ProjectId,
                dto.VersionId,
                userId,
                GetUserName());

            return Ok(draft);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto dto)
        {
            var userId = GetUserId();
            var result = await _emailApplicationService.SendAsync(
                dto,
                userId,
                GetUserName(),
                GetUserEmail());

            return Ok(result);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;

            throw new UnauthorizedException("User not authenticated.");
        }

        private string GetUserName()
        {
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            var fullName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrEmpty(fullName) ? "Applicant" : fullName;
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}