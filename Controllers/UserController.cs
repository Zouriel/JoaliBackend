using Joali.Data;
using JoaliBackend.DTO.UserDTOs;
using JoaliBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JoaliBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    

    public class UserController : ControllerBase
    {
        private readonly EFCoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserController(EFCoreDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [NonAction]
        public async Task<IResult> getUsers(string userid)
        {
            // check if the user exists 
            var user = await _context.Users.FindAsync(userid);
            if (user == null) return Results.NotFound();
            if (!user.IsActive) return Results.NotFound();

            return Results.Ok(user);
        }
        [HttpPost("CustomerRegister")]
        public async Task<IActionResult> NewCustomerRegistration([FromQuery] string APIKey,[FromBody]NewCustomerDTO NewCustomer)
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];

                if (APIKey != APIKEY) return BadRequest(new { message = "Invalid API Key" });
                // check for password match 
                if (NewCustomer.Password != NewCustomer.PasswordConfirm) return BadRequest(new { message = "Passwords do not match" });
                // check if user with the same email exists
                var existinguser = await _context.Users.FirstOrDefaultAsync(u => u.Email ==  NewCustomer.Email);
                if (existinguser != null) return BadRequest(new { message = "User with this email already exists" });
                //Hashed Password
                var PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewCustomer.Password);
                var customer = new User()
                {
                    Name = NewCustomer.Name,
                    Email = NewCustomer.Email,
                    Password_hash = PasswordHash,
                    IsActive = true,
                    UserType = UserType.Customer,
                    PhoneNumber = NewCustomer.Phone,
                    CreatedAt = DateTime.Now,
                };
                await _context.Users.AddAsync(customer);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Registration successful", data = customer });
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery]string ApiKey)
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];
                if (ApiKey != APIKEY) return BadRequest(new { message = "Invalid API Key" });
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("NewStaff")]
        public async Task<IActionResult> AddNewStaffAsync([FromBody] NewStaffDTO NewStaff, [FromQuery] string APIKey)
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];
                if (APIKey != APIKEY) return Unauthorized(new { message = "API key Unauthoried" });
                var InitialPassword = "Welcome123";
                var PasswordHash = BCrypt.Net.BCrypt.HashPassword(InitialPassword);

                // generate a new staff id
                var laststaffid = await _context.Users
                    .Where(u => u.staffId != null)
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => u.staffId)
                    .FirstOrDefaultAsync() ?? "JO0000";
                int lastNumber = int.Parse(laststaffid.Substring(4)); // Skip "ADM-"
                var newstaffid = "ADM-" + (lastNumber + 1).ToString("D3");
                if (NewStaff.OrgId != null)
                {
                    var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == NewStaff.OrgId);
                    if (org == null) return BadRequest(new { message = "Organization not found" });
                }

                var newstaff = new User()
                {
                    Name = NewStaff.Name,
                    Email = NewStaff.Email,
                    Password_hash = PasswordHash,
                    IsActive = false,
                    UserType = UserType.Staff,
                    StaffRole = StaffRole.Staff,
                    PhoneNumber = NewStaff.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    staffId = newstaffid,
                    OrgId = NewStaff.OrgId ?? null
                };
                await _context.Users.AddAsync(newstaff);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff added successfully", data = newstaff });

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [Authorize(Roles = "Admin")]
        [HttpPut("ToggleUser")]
        public async Task<IActionResult> ToggleUser([FromQuery] string APIkey, string Email)
        {
            try
            {
                var email = HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

                var staff = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (staff == null)
                    return BadRequest(new { message = "Request Made By a non Existent User" });

                if (staff.StaffRole != StaffRole.Admin)
                    return Unauthorized(new { message = "You are not authorized to perform this action" });

                var APIKEY = _configuration["API-KEY"];
                if (APIkey != APIKEY)
                    return BadRequest(new { message = "Invalid API Key" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (user == null)
                    return BadRequest(new { message = "User not found" });

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                var status = user.IsActive ? "activated" : "deactivated";
                return Ok(new { message = "User " + status + " successfully", data = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("debug-token")]
        public IActionResult DebugToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                return Unauthorized("🚫 Not authenticated or no token provided.");

            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                Name = user.Identity.Name,
                Claims = claims
            });
        }
        [HttpGet("SetStaffRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStaffRole([FromQuery] string APIKey, [FromQuery] string Email, [FromQuery] StaffRole Role)
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];
                if (APIKey != APIKEY) return BadRequest(new { message = "Invalid API Key" });
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (user == null) return BadRequest(new { message = "User not found" });
                user.StaffRole = Role;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff role set successfully", data = user });
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
