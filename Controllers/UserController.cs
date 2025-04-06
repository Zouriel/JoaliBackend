using Joali.Data;
using JoaliBackend.DTO.UserDTOs;
using JoaliBackend.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JoaliBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class UserController : ControllerBase
    {
        private readonly EFCoreDbContext _context;
        private readonly IConfiguration _configuration;
        public UserController(EFCoreDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [NonAction]
        public async Task<IResult> getUsers(string userid)
        {
            // check if the user exists 
            var user = await _context.Users.FindAsync(userid);
            if (user == null) return Results.NotFound();
            if (!user.IsACtive) return Results.NotFound();

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
                    IsACtive = true,
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
            var APIKEY = _configuration["API-KEY"];
            if (ApiKey != APIKEY) return BadRequest(new { message = "Invalid API Key" });
            var users = await _context.Users.Where(u => u.IsACtive == true).ToListAsync();
            return Ok(users);
        }

    }
}
