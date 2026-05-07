using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartResumeAnalyzer.Core.Configuration;
using SmartResumeAnalyzer.Core.DTOs.Auth;
using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Exceptions;
using SmartResumeAnalyzer.Core.Interfaces;
using SmartResumeAnalyzer.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _context;
        private readonly RateLimitSettings _rateLimitSettings;

        public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings, AppDbContext context, IOptions<RateLimitSettings> rateLimitSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _context = context;
            _rateLimitSettings = rateLimitSettings.Value;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedException("Account is deactivated.");

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new ConflictException("User with this email already exists.");

            var user = new User
            {
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim()
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            var today = DateTime.UtcNow.Date;

            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var cvVersionIds = await _context.CvVersions
                .Where(cv => cv.Project.UserId == userId)
                .Select(cv => cv.Id)
                .ToListAsync();

            var analyses = await _context.Analyses
                .Where(a => cvVersionIds.Contains(a.CvVersionId))
                .ToListAsync();

            var callCount = await _context.ApiUsage
                .Where(u => u.UserId == userId && u.Date == today)
                .Select(u => u.CallCount)
                .FirstOrDefaultAsync();

            var remainingToday = Math.Max(0, _rateLimitSettings.DailyLimitForUsers - callCount);

            return new UserProfileDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ReminderIntervalDays = user.ReminderIntervalDays,
                Stats = new UserStatsDto
                {
                    TotalProjects = projects.Count,
                    TotalCvVersions = cvVersionIds.Count,
                    TotalAnalyses = analyses.Count,
                    SentApplications = projects.Count(p => p.Status == "Sent"),
                    AcceptedApplications = projects.Count(p => p.Status == "Accepted"),
                    DeclinedApplications = projects.Count(p => p.Status == "Declined"),
                    AverageMatchScore = analyses.Count > 0 ? Math.Round(analyses.Average(a => a.MatchScore), 1) : 0,
                    RemainingAnalysesToday = remainingToday
                }
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.ReminderIntervalDays = dto.ReminderIntervalDays;

            await _userRepository.SaveChangesAsync();

            return await GetCurrentUserAsync(userId);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new AppException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepository.SaveChangesAsync();
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.ExpiryDays)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpiryDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}