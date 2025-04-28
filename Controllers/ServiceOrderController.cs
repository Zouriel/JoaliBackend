using Joali.Data;
using JoaliBackend.DTO.ServiceDTOs;
using JoaliBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoaliBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceOrderController : ControllerBase
    {
        private readonly EFCoreDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceOrderController(EFCoreDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 1️⃣ Place an Order (Booking or Purchase)
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceServiceOrderDTO dto)
        {
            try
            {
                var email = _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return Unauthorized(new { message = "User not found." });

                var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.IsActive);
                if (service == null) return BadRequest(new { message = "Service not available." });
                var Organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == service.OrgId);
                if (Organization == null) return BadRequest(new { message = "Organization not found." });

                var orderType = (service.DurationInMinutes.HasValue || dto.ScheduledFor.HasValue)
                    ? OrderType.Booking
                    : OrderType.Purchase;

                var order = new ServiceOrder
                {
                    ServiceId = dto.ServiceId,
                    OrgId = service.OrgId,
                    Service = service,
                    Organization = service.Organization,
                    UserId = user.Id,
                    Quantity = dto.Quantity,
                    ScheduledFor = dto.ScheduledFor,
                    CreatedAt = DateTime.UtcNow,
                    OrderType = orderType,
                    Status = orderType == OrderType.Purchase ? OrderStatus.Confirmed : OrderStatus.Pending
                };

                await _context.ServiceOrders.AddAsync(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Order placed successfully", data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to place order", error = ex.Message });
            }
        }

        // 2️⃣ User: Get My Orders
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var email = _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return Unauthorized(new { message = "User not found." });

                var orders = await _context.ServiceOrders
                    .Include(o => o.Service)
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve orders", error = ex.Message });
            }
        }

        // 3️⃣ Admin: View All Orders with Filters
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Staff,Manager")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int? orgId,
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to
        )
        {
            try
            {
                var query = _context.ServiceOrders
                    .Include(o => o.Service)
                    .Include(o => o.Service.Organization)
                    .AsQueryable();

                if (orgId.HasValue)
                    query = query.Where(o => o.OrgId == orgId.Value);

                if (status.HasValue)
                    query = query.Where(o => o.Status == status.Value);

                if (from.HasValue)
                    query = query.Where(o => o.CreatedAt >= from.Value);

                if (to.HasValue)
                    query = query.Where(o => o.CreatedAt <= to.Value);

                var results = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve orders", error = ex.Message });
            }
        }

        // 4️⃣ Admin: Update Order Status
        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin,Staff,Manager")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] OrderStatus status)
        {
            try
            {
                var order = await _context.ServiceOrders.FindAsync(id);
                if (order == null)
                    return NotFound(new { message = "Order not found." });
                if (IsAdmin(order.OrgId)) return Unauthorized(new { message = "You are not authorized to update this order." });

                order.Status = status;
                _context.ServiceOrders.Update(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Order status updated successfully", data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update order", error = ex.Message });
            }
        }

        private bool IsAdmin(int orgid)
        {
           var org = User.FindFirst("orgId")?.Value == orgid.ToString();
            return org;
        }
    }

}
