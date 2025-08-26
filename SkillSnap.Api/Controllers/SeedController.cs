using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;
namespace SkillSnap.Api.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class SeedController : ControllerBase
        {
                private readonly SkillSnapContext _context;
                public SeedController(SkillSnapContext context)
                {
                        _context = context;
                }
                [HttpPost]
                public async Task<IActionResult> Seed()
                {
                        // Use AsNoTracking for read-only check
                        var hasData = await _context.PortfolioUsers
                                .AsNoTracking()
                                .AnyAsync();
                        
                        if (hasData)
                        {
                                return BadRequest("Sample data already exists.");
                        }
                        var user = new PortfolioUser
                        {
                                        Name = "Jordan Developer",
                                        Bio = "Full-stack developer passionate about learning new tech.",
                                        ProfileImageUrl = "https://picsum.photos/200?random=1",
                                        Projects = new List<Project>
                                        {
                                                        new Project { Title = "Task Tracker", Description = "Manage tasks effectively", ImageUrl = "https://picsum.photos/200?random=2" },
                                                        new Project { Title = "Weather App", Description = "Forecast weather using APIs", ImageUrl = "https://picsum.photos/200?random=3" }
                                        },
                                        Skills = new List<Skill>
                                        {
                                                        new Skill { Name = "C#", Level = "Advanced" },
                                                        new Skill { Name = "Blazor", Level = "Intermediate" }
                                        }
                        };
                        _context.PortfolioUsers.Add(user);
                        await _context.SaveChangesAsync();
                        return Ok("Sample data inserted.");
                }

                [HttpDelete("all")]
                public async Task<IActionResult> DeleteAll()
                {
                        _context.Projects.RemoveRange(_context.Projects);
                        _context.Skills.RemoveRange(_context.Skills);
                        _context.PortfolioUsers.RemoveRange(_context.PortfolioUsers);
                        await _context.SaveChangesAsync();
                        return Ok("All data deleted.");
                }

        }
}