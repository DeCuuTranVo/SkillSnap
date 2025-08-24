using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;
using SkillSnap.Api.StaticDetails;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioUsersController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly ILogger<PortfolioUsersController> _logger;

        public PortfolioUsersController(SkillSnapContext context, ILogger<PortfolioUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get a specific portfolio user by ID
        /// </summary>
        /// <param name="id">Portfolio User ID</param>
        /// <returns>Portfolio user details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioUser>> GetPortfolioUser(int id)
        {
            try
            {
                var portfolioUser = await _context.PortfolioUsers.FindAsync(id);
                if (portfolioUser == null)
                {
                    return NotFound($"Portfolio user with ID {id} not found");
                }
                return Ok(portfolioUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving portfolio user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the portfolio user");
            }
        }

        /// <summary>
        /// Get all portfolio users
        /// </summary>
        /// <returns>List of portfolio users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUser>>> GetAllPortfolioUsers()
        {
            _logger.LogInformation("Retrieving all portfolio users");
            try
            {
                var users = await _context.PortfolioUsers.ToListAsync();
                _logger.LogInformation("Successfully retrieved {Count} portfolio users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all portfolio users");
                return StatusCode(500, "An error occurred while retrieving portfolio users");
            }
        }

        /// <summary>
        /// Create a new portfolio user
        /// </summary>
        /// <param name="portfolioUser">Portfolio user data</param>
        /// <returns>Created portfolio user</returns>
        [HttpPost]
        [Authorize] // Authenticated users can create portfolio users
        public async Task<ActionResult<PortfolioUser>> CreatePortfolioUser(PortfolioUser portfolioUser)
        {
            try
            {
                portfolioUser.Id = 0;
                _context.PortfolioUsers.Add(portfolioUser);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetPortfolioUser),
                    new { id = portfolioUser.Id },
                    portfolioUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating portfolio user");
                return StatusCode(500, "An error occurred while creating the portfolio user");
            }
        }

        /// <summary>
        /// Update an existing portfolio user
        /// </summary>
        /// <param name="id">Portfolio User ID</param>
        /// <param name="portfolioUser">Updated portfolio user data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize] // Authenticated users can update portfolio users
        public async Task<IActionResult> UpdatePortfolioUser(int id, PortfolioUser portfolioUser)
        {
            if (id != portfolioUser.Id)
            {
                return BadRequest("Portfolio user ID mismatch");
            }

            try
            {
                var existingUser = await _context.PortfolioUsers.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"Portfolio user with ID {id} not found");
                }

                existingUser.Name = portfolioUser.Name;
                existingUser.Bio = portfolioUser.Bio;
                existingUser.ProfileImageUrl = portfolioUser.ProfileImageUrl;
                // Add other property updates as needed

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating portfolio user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while updating the portfolio user");
            }
        }

        /// <summary>
        /// Delete a portfolio user
        /// </summary>
        /// <param name="id">Portfolio User ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleSD.Admin)] // Only Admin can delete portfolio users
        public async Task<IActionResult> DeletePortfolioUser(int id)
        {
            try
            {
                var portfolioUser = await _context.PortfolioUsers.FindAsync(id);
                if (portfolioUser == null)
                {
                    return NotFound($"Portfolio user with ID {id} not found");
                }

                _context.PortfolioUsers.Remove(portfolioUser);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting portfolio user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the portfolio user");
            }
        }
    }
}