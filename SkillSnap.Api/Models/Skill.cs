using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models;

public class Skill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Level { get; set; } = string.Empty;

    // Foreign key
    [ForeignKey(nameof(PortfolioUser))]
    public int PortfolioUserId { get; set; }

    // Navigation property
    public virtual PortfolioUser? PortfolioUser { get; set; }
}
