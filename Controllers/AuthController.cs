﻿using Joali.Data;
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
using System.ComponentModel.DataAnnotations;
using JoaliBackend.DTO.UserDTOs;

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

                if (!user.IsActive && user.UserType == UserType.Staff)
                {
                    var code = GenerateRandomDigits(23);
                    user.TemporaryKey = code;
                    user.TemporaryKeyExpiresAt = DateTime.UtcNow.AddMinutes(30); // temp key valid for 30 min
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    var responseData = new { email = user.Email, code = code };
                    return Ok(new { message = "RedirectToInitialPasswordPage", data = responseData });
                }
                if (!user.IsActive && user.UserType == UserType.Customer)
                    return BadRequest(new { message = "UserDeactivatedPAgeRedirect" });

                if (!BCrypt.Net.BCrypt.Verify(data.Password, user.Password_hash))
                    return BadRequest(new { message = "Invalid password" });

                var tokens = await GenerateToken(user);
                
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

                return Ok(new { message = "Login successful", token = tokens});
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
                return Ok(new { message = "Password reset successful"});
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

                var tokens =await  GenerateToken(user);

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
        [HttpGet("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery][EmailAddress(ErrorMessage = "Invalid email address")]string email, string  Apikey )
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];
                if(Apikey != APIKEY) return BadRequest(new { message = "Invalid API Key" });
                var user  = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if(user == null ) return NotFound(new { message = "User not found" });
                user.Password_hash = BCrypt.Net.BCrypt.HashPassword("Welcome123");
                user.IsActive = true;
                user.TemporaryKey = null;
                user.TemporaryKeyExpiresAt = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Password reset successful" });
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshtoken)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Missing or invalid token" });
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.token == token);
                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                {
                    if (session != null)
                    {
                        _context.Sessions.Remove(session);
                        await _context.SaveChangesAsync();
                    }
                    return Unauthorized(new { message = "Invalid or expired session" });    
                }
                if(session.RefreshToken != refreshtoken)
                    return Unauthorized(new { message = "Invalid or expired session" });
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == session.userId);
                if (user == null)
                    return Unauthorized(new { message = "User not found" });
                var tokens = await GenerateToken(user);
                var payload = new
                {
                    Access_token = tokens.AccessToken,
                    Refresh_Token = tokens.RefreshToken,
                };
                session.token = tokens.AccessToken;
                session.RefreshToken = tokens.RefreshToken;
                session.ExpiresAt = tokens.ExpiresAt;
                _context.Sessions.Update(session);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Token refreshed", New_token = payload });

            }catch(Exception Ex)
            {
                return Unauthorized(new { message = Ex.Message });
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

        private async Task<SecData> GenerateToken(User user)
        {
            var secretKey = _configuration["JWT:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var Booking = await _context.ServiceOrders.Where(b => b.UserId == user.Id).ToListAsync();
            Booking = Booking.Where(b => b.Status == OrderStatus.Confirmed).ToList();
            bool HasBooking = false;
            if (Booking.Any())
                HasBooking = true;
            else
                HasBooking = false;
            var claims = new[]
            {
                new Claim("sub", user.Email),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("name", user.Name ?? ""),
                new Claim("email", user.Email),
                new Claim("OrgId", user.OrgId.ToString() ?? ""),
                new Claim("role", user.UserType.ToString()),
                new Claim("staffRole", user.StaffRole?.ToString() ?? "None"),
                new Claim("hasBooking", HasBooking.ToString())
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
                ExpiresAt = DateTime.UtcNow.AddDays(1)
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