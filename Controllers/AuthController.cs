using Joali.Data;
using JoaliBackend.DTO.AuthDTOs;
using JoaliBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceDetectorNET;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JoaliBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EFCoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(EFCoreDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LogInDTO data)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var APIKEY = _configuration["API-KEY"];
                if (data.APIKEY != APIKEY)
                    return Unauthorized(new { message = "API key Unauthorized" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                         ?? httpContext.Connection.RemoteIpAddress?.ToString();

                var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
                var dd = new DeviceDetector(userAgent);
                dd.Parse();

                var deviceType = dd.GetDeviceName() ?? "Unknown";
                var os = dd.GetOs().Match?.Name ?? "Unknown";
                var client = dd.GetClient().Match?.Name ?? "Unknown";

                if (!user.IsActive)
                {
                    var code = GenerateRandomDigits(23);
                    user.TemporaryKey = code;
                    user.TemporaryKeyExpiresAt = DateTime.UtcNow.AddMinutes(30); // temp key valid for 30 min
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    var responseData = new { email = user.Email, code = code };
                    return Ok(new { message = "RedirectToInitialPasswordPage", data = responseData });
                }

                if (!BCrypt.Net.BCrypt.Verify(data.Password, user.Password_hash))
                    return BadRequest(new { message = "Invalid password" });

                var tokens = GenerateToken(user);

                var newSession = new Session()
                {
                    token = tokens.AccessToken,
                    userId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = tokens.ExpiresAt,
                    RefreshToken = tokens.RefreshToken,
                    IPAddress = ip ?? "unknown",
                    Device = deviceType,
                    Os = os,
                    Client = client,
                };

                await _context.Sessions.AddAsync(newSession);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Login successful", token = tokens });
            }
            catch
            {
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO data)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == data.Email);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                if (user.TemporaryKey != data.TemporaryKey || user.TemporaryKeyExpiresAt < DateTime.UtcNow)
                    return Unauthorized(new { message = "Invalid or expired temporary key" });

                if (!Regex.IsMatch(data.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
                    return BadRequest(new { message = "Password must be at least 8 characters long, contain upper and lower case letters, and at least one number." });

                user.Password_hash = BCrypt.Net.BCrypt.HashPassword(data.NewPassword);
                user.IsActive = true;
                user.TemporaryKey = null;
                user.TemporaryKeyExpiresAt = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var tokens = GenerateToken(user);
                var httpContext = _httpContextAccessor.HttpContext;

                var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                         ?? httpContext.Connection.RemoteIpAddress?.ToString();

                var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
                var dd = new DeviceDetector(userAgent);
                dd.Parse();

                var deviceType = dd.GetDeviceName() ?? "Unknown";
                var os = dd.GetOs().Match?.Name ?? "Unknown";
                var client = dd.GetClient().Match?.Name ?? "Unknown";

                var newSession = new Session()
                {
                    token = tokens.AccessToken,
                    userId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = tokens.ExpiresAt,
                    RefreshToken = tokens.RefreshToken,
                    IPAddress = ip ?? "unknown",
                    Device = deviceType,
                    Os = os,
                    Client = client,
                };

                await _context.Sessions.AddAsync(newSession);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Password reset successful", token = tokens });
            }
            catch
            {
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var accessToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                    return Unauthorized(new { message = "Missing or invalid token" });

                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.token == accessToken || s.RefreshToken == accessToken);

                if (session == null)
                    return NotFound(new { message = "Session not found or already logged out" });

                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Logout successful" });
            }
            catch
            {
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.AccessToken) || string.IsNullOrEmpty(data.RefreshToken))
                    return BadRequest(new { message = "Access token and refresh token are required" });

                var session = await _context.Sessions.FirstOrDefaultAsync(s =>
                    s.token == data.AccessToken && s.RefreshToken == data.RefreshToken);

                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                    return Unauthorized(new { message = "Invalid or expired session" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == session.userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                var tokens = GenerateToken(user);

                session.token = tokens.AccessToken;
                session.RefreshToken = tokens.RefreshToken;
                session.CreatedAt = DateTime.UtcNow;
                session.ExpiresAt = tokens.ExpiresAt;

                _context.Sessions.Update(session);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Token refreshed", token = tokens });
            }
            catch
            {
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }

        private string GenerateRandomDigits(int length)
        {
            var random = new Random();
            var digits = new char[length];

            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(digits);
        }

        private SecData GenerateToken(User user)
        {
            var secretKey = _configuration["JWT:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString()),
                new Claim("StaffRole", user.StaffRole?.ToString() ?? "None")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new SecData
            {
                AccessToken = accessToken,
                RefreshToken = GenerateRefreshToken(),
                ExpiresAt = token.ValidTo
            };
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}