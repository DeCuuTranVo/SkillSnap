namespace SkillSnap.Api.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;

public class SkillSnapContext : IdentityDbContext<ApplicationUser>
{
    public SkillSnapContext(DbContextOptions<SkillSnapContext> options) : base(options)
    { 
        
    }
    
    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PortfolioUser to Projects relationship (One-to-Many)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.PortfolioUser)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PortfolioUser to Skills relationship (One-to-Many)
        modelBuilder.Entity<Skill>()
            .HasOne(s => s.PortfolioUser)
            .WithMany(u => u.Skills)
            .HasForeignKey(s => s.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}