using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services
{
    public class UserSessionService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
        // User identity properties
        public string? UserId { get; private set; }
        public string? UserName { get; private set; }
        public string? Email { get; private set; }
        public string? Role { get; private set; }
        public int? PortfolioUserId { get; private set; }
        public bool IsAuthenticated { get; private set; }
        
        // Current editing/project state
        public Project? CurrentEditingProject { get; set; }
        public Skill? CurrentEditingSkill { get; set; }
        public PortfolioUser? CurrentEditingPortfolioUser { get; set; }
        
        // Navigation and UI state
        public string? LastVisitedPage { get; set; }
        public Dictionary<string, object> PageState { get; private set; } = new();
        public bool IsInEditMode { get; set; }
        public string? CurrentEditingMode { get; set; } // "project", "skill", "profile"
        
        // Search and filter state
        public string? LastSearchQuery { get; set; }
        public string? SelectedSkillFilter { get; set; }
        public string? SelectedProjectCategory { get; set; }
        
        // Temporary data for forms
        public Dictionary<string, string> FormData { get; private set; } = new();
        
        // Events for state changes
        public event Action? OnUserSessionChanged;
        public event Action? OnEditingStateChanged;
        public event Action? OnPageStateChanged;
        public event Action? OnFormDataChanged;
        public UserSessionService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
            
            // Subscribe to authentication state changes
            _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
            
            // Initialize user data
            _ = InitializeUserDataAsync();
        }

        private async void HandleAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await UpdateUserDataFromAuthStateAsync();
        }

        private async Task InitializeUserDataAsync()
        {
            await UpdateUserDataFromAuthStateAsync();
        }

        private async Task UpdateUserDataFromAuthStateAsync()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                var wasAuthenticated = IsAuthenticated;
                
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
                
                if (IsAuthenticated)
                {
                    UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    UserName = user.Identity?.Name;
                    Email = user.FindFirst(ClaimTypes.Email)?.Value;
                    Role = user.FindFirst(ClaimTypes.Role)?.Value;
                    
                    // Try to get PortfolioUserId from claims
                    var portfolioUserIdClaim = user.FindFirst("PortfolioUserId")?.Value;
                    if (int.TryParse(portfolioUserIdClaim, out var portfolioUserId))
                    {
                        PortfolioUserId = portfolioUserId;
                    }
                }
                else
                {
                    // Clear user data on logout
                    ClearUserData();
                }
                
                // Notify subscribers if authentication state changed
                if (wasAuthenticated != IsAuthenticated)
                {
                    OnUserSessionChanged?.Invoke();
                }
            }
            catch
            {
                // If there's an error, clear the user data
                ClearUserData();
                OnUserSessionChanged?.Invoke();
            }
        }

        private void ClearUserData()
        {
            UserId = null;
            UserName = null;
            Email = null;
            Role = null;
            PortfolioUserId = null;
            IsAuthenticated = false;
            
            // Clear editing state
            ClearEditingState();
            
            // Clear page state but keep navigation state
            PageState.Clear();
            FormData.Clear();
        }

        #region User Role Helpers
        
        public bool IsInRole(string role)
        {
            return IsAuthenticated && string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool IsAdmin()
        {
            return IsInRole("Admin");
        }
        
        public bool IsUser()
        {
            return IsInRole("User");
        }
        
        #endregion

        #region Editing State Management
        
        public void StartEditingProject(Project? project = null)
        {
            ClearEditingState();
            CurrentEditingProject = project;
            CurrentEditingMode = "project";
            IsInEditMode = true;
            OnEditingStateChanged?.Invoke();
        }
        
        public void StartEditingSkill(Skill? skill = null)
        {
            ClearEditingState();
            CurrentEditingSkill = skill;
            CurrentEditingMode = "skill";
            IsInEditMode = true;
            OnEditingStateChanged?.Invoke();
        }
        
        public void StartEditingProfile(PortfolioUser? portfolioUser = null)
        {
            ClearEditingState();
            CurrentEditingPortfolioUser = portfolioUser;
            CurrentEditingMode = "profile";
            IsInEditMode = true;
            OnEditingStateChanged?.Invoke();
        }
        
        public void ClearEditingState()
        {
            CurrentEditingProject = null;
            CurrentEditingSkill = null;
            CurrentEditingPortfolioUser = null;
            CurrentEditingMode = null;
            IsInEditMode = false;
            OnEditingStateChanged?.Invoke();
        }
        
        public bool IsEditing(string type)
        {
            return IsInEditMode && string.Equals(CurrentEditingMode, type, StringComparison.OrdinalIgnoreCase);
        }
        
        #endregion

        #region Form Data Management
        
        public void SetFormData(string key, string value)
        {
            FormData[key] = value;
        }
        
        public string? GetFormData(string key)
        {
            return FormData.TryGetValue(key, out var value) ? value : null;
        }
        
        public void ClearFormData(string? key = null)
        {
            if (key == null)
            {
                FormData.Clear();
            }
            else
            {
                FormData.Remove(key);
            }
        }
        
        #endregion

        #region User Display Helpers
        
        public string GetDisplayName()
        {
            return UserName ?? Email ?? "Unknown User";
        }
        
        public string GetUserInitials()
        {
            var displayName = GetDisplayName();
            if (string.IsNullOrEmpty(displayName))
                return "??";
                
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            else
                return displayName.Length >= 2 ? displayName[..2].ToUpper() : displayName.ToUpper();
        }
        
        public string GetRoleDisplayName()
        {
            return Role switch
            {
                "Admin" => "Administrator",
                "User" => "User",
                _ => "Guest"
            };
        }
        
        #endregion

        #region Page Navigation and State Management
        
        public void SetLastVisitedPage(string page)
        {
            LastVisitedPage = page;
        }
        
        public void SetPageState(string key, object value)
        {
            PageState[key] = value;
            OnPageStateChanged?.Invoke();
        }
        
        public T? GetPageState<T>(string key)
        {
            if (PageState.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default(T);
        }
        
        public void ClearPageState(string? key = null)
        {
            if (key == null)
            {
                PageState.Clear();
            }
            else
            {
                PageState.Remove(key);
            }
            OnPageStateChanged?.Invoke();
        }
        
        public void SetSearchQuery(string query)
        {
            LastSearchQuery = query;
        }
        
        public void SetSkillFilter(string filter)
        {
            SelectedSkillFilter = filter;
        }
        
        public void SetProjectCategory(string category)
        {
            SelectedProjectCategory = category;
        }
        
        public void ClearSearchState()
        {
            LastSearchQuery = null;
            SelectedSkillFilter = null;
            SelectedProjectCategory = null;
        }
        
        #endregion

        #region Session Persistence (for page refreshes)
        
        public void SaveSessionToStorage()
        {
            // This would typically use localStorage to persist session state
            // across page refreshes, but since we have JWT tokens handling
            // authentication persistence, we'll keep this simple for now
        }
        
        public void LoadSessionFromStorage()
        {
            // Load any persistent session state from localStorage
            // This could include last visited page, search queries, etc.
        }
        
        #endregion

        // Cleanup
        public void Dispose()
        {
            _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
        }
    }
}