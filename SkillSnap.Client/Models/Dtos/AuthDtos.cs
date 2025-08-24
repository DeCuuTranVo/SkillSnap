using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Client.Models.Dtos
{
    // DTOs for authentication
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public bool Success { get; set; }  // API uses "Success" instead of "IsSuccess"
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int PortfolioUserId { get; set; }  // API also returns this
        
        // Computed property that checks both Success and IsSuccess
        public bool IsLoginSuccessful => Success || IsSuccess;
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public bool Success { get; set; }  // API uses "Success" instead of "IsSuccess"
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        
        // Computed property that checks both Success and IsSuccess
        public bool IsRegistrationSuccessful => Success || IsSuccess;
    }

    public class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
