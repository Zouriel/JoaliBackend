using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JoaliBackend.Models; 
using Joali.Data;
using JoaliBackend.DTOs; 

namespace JoaliBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly EFCoreDbContext _context;

        public OrganizationController(EFCoreDbContext context)
        {
            _context = context;
        }
        // GET: api/Organizations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations([FromQuery] int? orgtype = null)
        {
            

            if (orgtype.HasValue)
            {
                if (!Enum.IsDefined(typeof(OrgType), orgtype.Value))
                    return BadRequest("Invalid organization type.");

                var filtered = await _context.Organizations
                                             .Where(o => o.Type == (OrgType)orgtype.Value)
                                             .ToListAsync();
                return Ok(filtered);
            }

            var allOrgs = await _context.Organizations.ToListAsync();
            return Ok(allOrgs);
        }

        
        // GET: api/Organization/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganization(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            return organization;
        }

        // Fix the issue by correcting the property name in the OrganizationDto mapping.
        // The property in OrganizationDto is named `orgType`, not `Type`.

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Organization>> CreateOrganization([FromBody] OrganizationDto dto)
        {
            if (!IsAdmin())
            {
                return Forbid("Only Admins can create organizations.");
            }
            var initiman = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.InitialManager);
            if (initiman == null) return BadRequest(new { message = "Initial manager not found." });
            var organization = new Organization
            {
                Name = dto.Name,
                RegistrationNumber = dto.RegistrationNumber,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Country = dto.Country,
                LogoUrl = dto.LogoUrl,
                Website = dto.Website,
                Type = dto.orgType,
                CreatedAt = DateTime.UtcNow
            };
            


            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            initiman.OrgId = organization.Id;
            initiman.StaffRole = StaffRole.Manager;
            _context.Users.Update(initiman);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
        }


        // POST: api/Organization/toggle/5
        [HttpPut("toggle/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleOrganization(int id)
        {
            if (!IsAdmin())
            {
                return Forbid("Only Admins can toggle organization status.");
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound(new { message = "Organization not found." });
            }

            organization.IsActive = !organization.IsActive; // 🔄 Flip the switch
            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();

            var status = organization.IsActive ? "activated" : "deactivated";
            return Ok(new { message = $"Organization has been {status}." });
        }


        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }

        private bool IsAdmin()
        {
            var role = User.FindFirst("staffRole")?.Value; 
            return role == "Admin";
        }
    }
}
