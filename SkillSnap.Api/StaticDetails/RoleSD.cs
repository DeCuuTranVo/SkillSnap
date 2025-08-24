namespace SkillSnap.Api.StaticDetails;

/// <summary>
/// Static Details class for Role constants
/// Centralizes all role definitions for consistent usage across the application
/// </summary>
public static class RoleSD
{
    /// <summary>
    /// Administrator role - full system access
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Regular user role - basic access
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Moderator role - limited administrative access
    /// </summary>
    public const string Moderator = "Moderator";

    /// <summary>
    /// Portfolio Owner role - can manage their own portfolio
    /// </summary>
    public const string PortfolioOwner = "PortfolioOwner";

    /// <summary>
    /// Gets all available roles as an array
    /// </summary>
    /// <returns>Array of all role names</returns>
    public static string[] GetAllRoles()
    {
        return new string[] { Admin, User, Moderator, PortfolioOwner };
    }

    /// <summary>
    /// Gets the default role assigned to new users
    /// </summary>
    /// <returns>Default role name</returns>
    public static string GetDefaultRole()
    {
        return User;
    }

    /// <summary>
    /// Gets administrative roles
    /// </summary>
    /// <returns>Array of administrative role names</returns>
    public static string[] GetAdminRoles()
    {
        return new string[] { Admin, Moderator };
    }

    /// <summary>
    /// Checks if a role is an administrative role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if the role is administrative</returns>
    public static bool IsAdminRole(string role)
    {
        return role == Admin || role == Moderator;
    }

    /// <summary>
    /// Checks if a role is valid
    /// </summary>
    /// <param name="role">Role name to validate</param>
    /// <returns>True if the role exists in the system</returns>
    public static bool IsValidRole(string role)
    {
        var allRoles = GetAllRoles();
        return Array.Exists(allRoles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
