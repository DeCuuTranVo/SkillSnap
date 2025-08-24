# RoleSD Usage Examples

The `RoleSD` class provides centralized role management for the SkillSnap API. Here are examples of how to use it:

## Controller Authorization Examples

```csharp
using Microsoft.AspNetCore.Authorization;
using SkillSnap.Api.StaticDetails;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // Only Admin role can access
    [Authorize(Roles = RoleSD.Admin)]
    [HttpGet("system-stats")]
    public async Task<IActionResult> GetSystemStats() { }

    // Admin or Moderator can access
    [Authorize(Roles = $"{RoleSD.Admin},{RoleSD.Moderator}")]
    [HttpPost("moderate-content")]
    public async Task<IActionResult> ModerateContent() { }

    // Any authenticated user can access
    [Authorize(Roles = RoleSD.User)]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() { }
}
```

## Service Layer Role Checking

```csharp
public class UserService
{
    public bool CanModerateContent(string userRole)
    {
        return RoleSD.IsAdminRole(userRole);
    }

    public string[] GetAvailableRoles()
    {
        return RoleSD.GetAllRoles();
    }

    public bool IsValidRole(string role)
    {
        return RoleSD.IsValidRole(role);
    }
}
```

## Role Assignment Examples

```csharp
// Assign admin role
await _userManager.AddToRoleAsync(user, RoleSD.Admin);

// Assign default role to new users
await _userManager.AddToRoleAsync(user, RoleSD.GetDefaultRole());

// Check if user has admin privileges
var userRoles = await _userManager.GetRolesAsync(user);
bool isAdmin = userRoles.Any(r => RoleSD.IsAdminRole(r));
```

## Custom Authorization Policies

```csharp
// In Program.cs or ServiceExtensions.cs
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole(RoleSD.Admin));
        
    options.AddPolicy("AdminOrModerator", policy => 
        policy.RequireRole(RoleSD.GetAdminRoles()));
        
    options.AddPolicy("PortfolioOwnerOrAdmin", policy =>
        policy.RequireRole(RoleSD.PortfolioOwner, RoleSD.Admin));
});

// Usage in controllers
[Authorize(Policy = "AdminOnly")]
[HttpDelete("delete-user/{id}")]
public async Task<IActionResult> DeleteUser(int id) { }
```

## JWT Claims Validation

```csharp
public class TokenService
{
    public bool ValidateUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return !string.IsNullOrEmpty(roleClaim) && RoleSD.IsValidRole(roleClaim);
    }

    public bool UserHasAdminAccess(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return !string.IsNullOrEmpty(roleClaim) && RoleSD.IsAdminRole(roleClaim);
    }
}
```

## Benefits of Using RoleSD

1. **Centralized Management**: All roles defined in one place
2. **Type Safety**: No magic strings scattered throughout code
3. **Easy Maintenance**: Change role names in one location
4. **IntelliSense Support**: IDE autocomplete for role names
5. **Validation Methods**: Built-in role validation logic
6. **Future-Proof**: Easy to add new roles and methods
