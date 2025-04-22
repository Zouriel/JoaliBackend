using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JoaliBackend.Models; // adjust namespace
using Joali.Data;
using JoaliBackend.DTOs; // adjust if DbContext is elsewhere

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

        // GET: api/Organization
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations([FromQuery] int? orgtype = null)
        {
            if (!IsAdmin())
                return Forbid("Only Admins can view organizations.");

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
        public async Task<ActionResult<Organization>> CreateOrganization([FromBody] OrganizationDto dto)
        {
            if (!IsAdmin())
            {
                return Forbid("Only Admins can create organizations.");
            }

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
                Type = (OrgType)dto.orgType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
        }


        // DELETE: api/Organization/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            if (!IsAdmin())
            {
                return Forbid("Only Admins can delete organizations.");
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            organization.IsActive = false;
            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();

            return NoContent();
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
