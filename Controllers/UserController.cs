﻿using Joali.Data;
using JoaliBackend.DTO.UserDTOs;
using JoaliBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;


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
                var users = await _context.Users.Where(u => u.IsActive == true).ToListAsync();
                return Ok(users);
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

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
                int lastNumber = int.Parse(laststaffid.Substring(2));
                var newstaffid = "JO" + (lastNumber + 1).ToString("D4");

                var newstaff = new User()
                {
                    Name = NewStaff.Name,
                    Email = NewStaff.Email,
                    Password_hash = PasswordHash,
                    IsActive = false,
                    UserType = UserType.Staff,
                    PhoneNumber = NewStaff.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    staffId = newstaffid,
                };
                await _context.Users.AddAsync(newstaff);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Staff added successfully", data = newstaff });

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpPost("DisableUser")]
        public async Task<IActionResult> DisableUser([FromQuery] string APIkey,string StaffId, string userId)
        {
            try
            {
                var APIKEY = _configuration["API-KEY"];
                if(APIkey != APIKEY) 
                    return Unauthorized(new { message = "API key Unauthoried" });
                var requestingstaff = await _context.Users.FindAsync(StaffId);
                if (requestingstaff == null || requestingstaff.StaffRole != StaffRole.Admin) 
                    return Unauthorized(new { message = "You are not authorized to perform this action" });
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User Not Fount" });
                user.IsActive = false;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User disabled successfully" });
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
