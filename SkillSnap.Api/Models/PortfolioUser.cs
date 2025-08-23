using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Api.Models;

public class PortfolioUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [StringLength(500)]
    public string ProfileImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
