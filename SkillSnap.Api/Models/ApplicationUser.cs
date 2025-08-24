using Microsoft.AspNetCore.Identity;

namespace SkillSnap.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int PortfolioUserId { get; set; }
    }
}