using SkillSnap.Api.Extensions;
using SkillSnap.Api.Data;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;
using Microsoft.AspNetCore.Identity;
using SkillSnap.Api.StaticDetails;

var builder = WebApplication.CreateBuilder(args);

// Add all application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Seed data for testing
await SeedTestDataAsync(app);

// Seed roles for authentication
await SeedRolesAsync(app);

// Configure the HTTP request pipeline
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Method to seed test data
async Task SeedTestDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SkillSnapContext>();
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Check if there's already data
    if (!await context.PortfolioUsers.AnyAsync())
    {
        var testUser = new PortfolioUser
        {
            Name = "John Doe",
            Bio = "Full Stack Developer with 5 years of experience",
            ProfileImageUrl = "https://picsum.photos/200/200?random=1"
        };
        
        context.PortfolioUsers.Add(testUser);
        await context.SaveChangesAsync();
        
        var projects = new List<Project>
        {
            new Project
            {
                Title = "E-Commerce Platform",
                Description = "A modern e-commerce platform built with React and Node.js",
                ImageUrl = "https://picsum.photos/400/300?random=2",
                PortfolioUserId = testUser.Id
            },
            new Project
            {
                Title = "Task Management App",
                Description = "A collaborative task management application with real-time updates",
                ImageUrl = "https://picsum.photos/400/300?random=3",
                PortfolioUserId = testUser.Id
            }
        };
        
        context.Projects.AddRange(projects);
        
        var skills = new List<Skill>
        {
            new Skill
            {
                Name = "C#",
                Level = "Expert",
                PortfolioUserId = testUser.Id
            },
            new Skill
            {
                Name = "React",
                Level = "Advanced",
                PortfolioUserId = testUser.Id
            },
            new Skill
            {
                Name = "Node.js",
                Level = "Advanced",
                PortfolioUserId = testUser.Id
            }
        };
        
        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }
}

// Method to seed roles
async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Get all roles from RoleSD static class
    var roles = RoleSD.GetAllRoles();
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}