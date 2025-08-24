using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly SkillSnapContext _context;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(SkillSnapContext context, ILogger<ProjectsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    /// <returns>List of all projects</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        try
        {
            var projects = await _context.Projects
                .Include(p => p.PortfolioUser)
                .ToListAsync();

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving projects");
            return StatusCode(500, "An error occurred while retrieving projects");
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.PortfolioUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving project with ID {ProjectId}", id);
            return StatusCode(500, "An error occurred while retrieving the project");
        }
    }

    /// <summary>
    /// Get projects by portfolio user ID
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>List of projects for the specified user</returns>
    [HttpGet("user/{portfolioUserId}")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByUser(int portfolioUserId)
    {
        try
        {
            // First check if the user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == portfolioUserId);
            if (!userExists)
            {
                return NotFound($"Portfolio user with ID {portfolioUserId} not found");
            }

            var projects = await _context.Projects
                .Include(p => p.PortfolioUser)
                .Where(p => p.PortfolioUserId == portfolioUserId)
                .ToListAsync();

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving projects for user {UserId}", portfolioUserId);
            return StatusCode(500, "An error occurred while retrieving user projects");
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="project">Project data</param>
    /// <returns>Created project</returns>
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        try
        {
            // Validate that the portfolio user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == project.PortfolioUserId);
            if (!userExists)
            {
                return BadRequest($"Portfolio user with ID {project.PortfolioUserId} does not exist");
            }

            // Reset ID to ensure it's auto-generated
            project.Id = 0;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Reload the project with navigation properties
            var createdProject = await _context.Projects
                .Include(p => p.PortfolioUser)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                createdProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating project");
            return StatusCode(500, "An error occurred while creating the project");
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="project">Updated project data</param>
    /// <returns>Updated project</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, Project project)
    {
        if (id != project.Id)
        {
            return BadRequest("Project ID mismatch");
        }

        try
        {
            // Check if project exists
            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            // Validate that the portfolio user exists
            var userExists = await _context.PortfolioUsers.AnyAsync(u => u.Id == project.PortfolioUserId);
            if (!userExists)
            {
                return BadRequest($"Portfolio user with ID {project.PortfolioUserId} does not exist");
            }

            // Update properties
            existingProject.Title = project.Title;
            existingProject.Description = project.Description;
            existingProject.ImageUrl = project.ImageUrl;
            existingProject.PortfolioUserId = project.PortfolioUserId;

            await _context.SaveChangesAsync();

            // NoContent() returns HTTP status code 204
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating project with ID {ProjectId}", id);
            return StatusCode(500, "An error occurred while updating the project");
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project with ID {ProjectId}", id);
            return StatusCode(500, "An error occurred while deleting the project");
        }
    }
}