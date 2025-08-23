using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models;

public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    // Foreign key
    [ForeignKey(nameof(PortfolioUser))]
    public int PortfolioUserId { get; set; }

    // Navigation property
    public virtual PortfolioUser PortfolioUser { get; set; } = null!;
}
