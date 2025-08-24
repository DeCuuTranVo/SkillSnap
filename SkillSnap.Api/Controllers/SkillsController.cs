using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;
using SkillSnap.Api.StaticDetails;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly SkillSnapContext _context;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(SkillSnapContext context, ILogger<SkillsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all skills
    /// </summary>
    /// <returns>List of all skills</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
    {
        try
        {
            var skills = await _context.Skills
                .Include(s => s.PortfolioUser)
                .ToListAsync();

            return Ok(skills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skills");
            return StatusCode(500, "An error occurred while retrieving skills");
        }
    }

    /// <summary>
    /// Get a specific skill by ID
    /// </summary>
    /// <param name="id">Skill ID</param>
    /// <returns>Skill details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Skill>> GetSkill(int id)
    {
        try
        {
            var skill = await _context.Skills
                .Include(s => s.PortfolioUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (skill == null)
            {
                return NotFound($"Skill with ID {id} not found");
            }

            return Ok(skill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skill with ID {SkillId}", id);
            return StatusCode(500, "An error occurred while retrieving the skill");
        }
    }

    /// <summary>
    /// Get skills by portfolio user ID
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>List of skills for the specified user</returns>
    [HttpGet("user/{portfolioUserId}")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkillsByUser(int portfolioUserId)
    {
        try
        {
            // First check if the user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == portfolioUserId);
            if (!userExists)
            {
                return NotFound($"Portfolio user with ID {portfolioUserId} not found");
            }

            var skills = await _context.Skills
                .Include(s => s.PortfolioUser)
                .Where(s => s.PortfolioUserId == portfolioUserId)
                .ToListAsync();

            return Ok(skills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skills for user {UserId}", portfolioUserId);
            return StatusCode(500, "An error occurred while retrieving user skills");
        }
    }

    /// <summary>
    /// Get skills by skill level
    /// </summary>
    /// <param name="level">Skill level (e.g., Beginner, Intermediate, Advanced, Professional)</param>
    /// <returns>List of skills with the specified level</returns>
    [HttpGet("level/{level}")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkillsByLevel(string level)
    {
        try
        {
            var skills = await _context.Skills
                .Include(s => s.PortfolioUser)
                .Where(s => s.Level.ToLower() == level.ToLower())
                .ToListAsync();

            return Ok(skills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving skills with level {Level}", level);
            return StatusCode(500, "An error occurred while retrieving skills by level");
        }
    }

    /// <summary>
    /// Create a new skill
    /// </summary>
    /// <param name="skill">Skill data</param>
    /// <returns>Created skill</returns>
    [HttpPost]
    [Authorize] // Authenticated users can create skills
    public async Task<ActionResult<Skill>> CreateSkill(Skill skill)
    {
        try
        {
            // Validate that the portfolio user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == skill.PortfolioUserId);
            if (!userExists)
            {
                return BadRequest($"Portfolio user with ID {skill.PortfolioUserId} does not exist");
            }

            // Validate skill level
            if (!IsValidSkillLevel(skill.Level))
            {
                return BadRequest("Invalid skill level. Valid levels are: Beginner, Intermediate, Advanced, Professional");
            }

            // Reset ID to ensure it's auto-generated
            skill.Id = 0;

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            // Reload the skill with navigation properties
            var createdSkill = await _context.Skills
                .Include(s => s.PortfolioUser)
                .FirstOrDefaultAsync(s => s.Id == skill.Id);

            return CreatedAtAction(
                nameof(GetSkill),
                new { id = skill.Id },
                createdSkill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating skill");
            return StatusCode(500, "An error occurred while creating the skill");
        }
    }

    /// <summary>
    /// Update an existing skill
    /// </summary>
    /// <param name="id">Skill ID</param>
    /// <param name="skill">Updated skill data</param>
    /// <returns>Updated skill</returns>
    [HttpPut("{id}")]
    [Authorize] // Authenticated users can update skills
    public async Task<IActionResult> UpdateSkill(int id, Skill skill)
    {
        if (id != skill.Id)
        {
            return BadRequest("Skill ID mismatch");
        }

        try
        {
            // Check if skill exists
            var existingSkill = await _context.Skills.FindAsync(id);
            if (existingSkill == null)
            {
                return NotFound($"Skill with ID {id} not found");
            }

            // Validate that the portfolio user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == skill.PortfolioUserId);
            if (!userExists)
            {
                return BadRequest($"Portfolio user with ID {skill.PortfolioUserId} does not exist");
            }

            // Validate skill level
            if (!IsValidSkillLevel(skill.Level))
            {
                return BadRequest("Invalid skill level. Valid levels are: Beginner, Intermediate, Advanced, Professional");
            }

            // Update properties
            existingSkill.Name = skill.Name;
            existingSkill.Level = skill.Level;
            existingSkill.PortfolioUserId = skill.PortfolioUserId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating skill with ID {SkillId}", id);
            return StatusCode(500, "An error occurred while updating the skill");
        }
    }

    /// <summary>
    /// Delete a skill
    /// </summary>
    /// <param name="id">Skill ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleSD.Admin)] // Only Admin can delete skills
    public async Task<IActionResult> DeleteSkill(int id)
    {
        try
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with ID {id} not found");
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting skill with ID {SkillId}", id);
            return StatusCode(500, "An error occurred while deleting the skill");
        }
    }

    /// <summary>
    /// Get available skill levels
    /// </summary>
    /// <returns>List of valid skill levels</returns>
    [HttpGet("levels")]
    public ActionResult<IEnumerable<string>> GetSkillLevels()
    {
        var skillLevels = new List<string>
        {
            "Beginner",
            "Intermediate", 
            "Advanced",
            "Professional"
        };

        return Ok(skillLevels);
    }

    /// <summary>
    /// Validate if the skill level is valid
    /// </summary>
    /// <param name="level">Skill level to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValidSkillLevel(string level)
    {
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "Professional" };
        return validLevels.Any(vl => string.Equals(vl, level, StringComparison.OrdinalIgnoreCase));
    }
}