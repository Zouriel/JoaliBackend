using Joali.Data;
using JoaliBackend.DTO.ServiceDTOs;
using JoaliBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoaliBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly EFCoreDbContext _context;

        public ServiceController(EFCoreDbContext context)
        {
            _context = context;
        }

        // 1️⃣ [ADMIN ONLY] Create a new service
        [HttpPost("create")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateService([FromBody] NewServiceDTO dto)
        {
            try
            {
                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == dto.OrgId);
                if (org == null) return BadRequest(new { message = "Organization not found" });
                var serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(s => s.Id == dto.ServiceTypeId);
                if (serviceType == null) return BadRequest(new { message = "Service type not found" });
                var service = new Service
                {
                    Name = dto.Name,
                    Description = dto.Description ?? "",
                    Price = dto.Price,
                    OrgId = dto.OrgId,
                    Organization = org,
                    ServiceTypeId = dto.ServiceTypeId,
                    ServiceType = serviceType,
                    Capacity = dto.Capacity,
                    CreatedAt = DateTime.UtcNow,
                    DurationInMinutes = dto.DurationInMinutes,
                    imageUrl = dto.imageUrl,
                    IsActive = true
                };

                await _context.Services.AddAsync(service);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Service created successfully", data = service });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create service", error = ex.Message });
            }
        }

        // 2️⃣ [ADMIN ONLY] Toggle soft delete (IsActive)
        [HttpPost("toggle/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ToggleServiceActive(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                    return NotFound(new { message = "Service not found" });

                service.IsActive = !service.IsActive;
                _context.Services.Update(service);
                await _context.SaveChangesAsync();

                var status = service.IsActive ? "activated" : "deactivated";
                return Ok(new { message = $"Service {status} successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to toggle service status", error = ex.Message });
            }
        }

        // 3️⃣ [OPEN] Get all services with optional org/type filters
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices([FromQuery] int? orgId = null, [FromQuery] int? typeId = null)
        {
            try
            {
                var query = _context.Services
                    .Include(s => s.Organization)
                    .Include(s => s.ServiceType)
                    .AsQueryable();

                if (orgId.HasValue)
                    query = query.Where(s => s.OrgId == orgId.Value);

                if (typeId.HasValue)
                    query = query.Where(s => s.ServiceTypeId == typeId.Value);

                var services = await query.ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve services", error = ex.Message });
            }
        }
        // 4️⃣ [ADMIN ONLY] Create a new Service Type
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("create-service-type")]
        public async Task<IActionResult> CreateServiceType([FromBody] NewServiceTypeDTO dto)
        {
            try
            {
                var existing = await _context.ServiceTypes
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower());

                if (existing != null)
                    return BadRequest(new { message = "A service type with this name already exists." });

                var newType = new ServiceType
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                await _context.ServiceTypes.AddAsync(newType);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Service type created successfully", data = newType });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create service type", error = ex.Message });
            }
        }
        // 5️⃣ [AUTHORIZED] Get all service types
        [HttpGet("all-service-types")]
        [Authorize]
        public async Task<IActionResult> GetAllServiceTypes()
        {
            try
            {
                var types = await _context.ServiceTypes.ToListAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve service types", error = ex.Message });
            }
        }

        //payment simulate endpoint (Temporary)
        [HttpPost("Pay")]
        [Authorize]
        public async Task<IActionResult> Pay([FromBody] int serviceId)
        {
            try
            {
                var email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                if (email == null) return Unauthorized(new { message = "User not found" });
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return BadRequest(new { message = "User not found" });
                var serviceOrder = await _context.ServiceOrders.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (serviceOrder == null) return BadRequest(new { message = "Service not found" });
                var IsValid = user.Id == serviceOrder.UserId;
                if (!IsValid) return Unauthorized(new { message = "Invalid user" });
                serviceOrder.Status = OrderStatus.Confirmed;
                _context.ServiceOrders.Update(serviceOrder);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Payment successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to make payment", error = ex.Message });
            }
        }


    }
}
