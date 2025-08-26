# UserSessionService Documentation

## Overview

The `UserSessionService` is a scoped state container service that stores user information and application state across components without requiring page reloads. It provides a centralized way to manage user session data, editing states, form data, and navigation context.

## Registration

The service is registered as a scoped service in `Program.cs`:

```csharp
builder.Services.AddScoped<UserSessionService>();
```

## Features

### 1. **User Identity Management**
- Automatically syncs with `AuthenticationStateProvider`
- Stores user ID, username, email, role, and portfolio user ID
- Provides role-based helper methods
- Handles authentication state changes

### 2. **Editing State Management**
- Tracks current editing mode (project, skill, profile)
- Stores currently editing entities
- Provides state change notifications
- Manages edit mode transitions

### 3. **Page State Persistence**
- Stores arbitrary page state data
- Tracks last visited page
- Provides key-value storage for UI state
- Maintains state across component navigation

### 4. **Form Data Management**
- Temporary storage for form data
- Useful for multi-step forms
- Data persistence during navigation
- Easy cleanup mechanisms

### 5. **Search and Filter State**
- Remembers search queries
- Stores filter selections
- Maintains user preferences
- Cross-component search state

## Core Properties

### User Identity
```csharp
public string? UserId { get; private set; }
public string? UserName { get; private set; }
public string? Email { get; private set; }
public string? Role { get; private set; }
public int? PortfolioUserId { get; private set; }
public bool IsAuthenticated { get; private set; }
```

### Editing State
```csharp
public Project? CurrentEditingProject { get; set; }
public Skill? CurrentEditingSkill { get; set; }
public PortfolioUser? CurrentEditingPortfolioUser { get; set; }
public bool IsInEditMode { get; set; }
public string? CurrentEditingMode { get; set; }
```

### Navigation and State
```csharp
public string? LastVisitedPage { get; set; }
public Dictionary<string, object> PageState { get; private set; }
public Dictionary<string, string> FormData { get; private set; }
```

## Key Methods

### User Role Helpers
```csharp
bool IsInRole(string role)        // Check if user has specific role
bool IsAdmin()                    // Check if user is admin
bool IsUser()                     // Check if user is regular user
string GetDisplayName()           // Get user's display name
string GetUserInitials()          // Get user's initials for avatars
string GetRoleDisplayName()       // Get formatted role name
```

### Editing State Management
```csharp
void StartEditingProject(Project? project = null)     // Enter project edit mode
void StartEditingSkill(Skill? skill = null)           // Enter skill edit mode
void StartEditingProfile(PortfolioUser? user = null)  // Enter profile edit mode
void ClearEditingState()                              // Exit edit mode
bool IsEditing(string type)                           // Check current edit mode
```

### Page State Management
```csharp
void SetPageState(string key, object value)  // Store page state
T? GetPageState<T>(string key)               // Retrieve page state
void ClearPageState(string? key = null)      // Clear state
void SetLastVisitedPage(string page)         // Track navigation
```

### Form Data Management
```csharp
void SetFormData(string key, string value)   // Store form data
string? GetFormData(string key)              // Retrieve form data
void ClearFormData(string? key = null)       // Clear form data
```

### Search and Filter State
```csharp
void SetSearchQuery(string? query)           // Store search query
void SetSkillFilter(string? filter)          // Store skill filter
void SetProjectCategory(string? category)    // Store project filter
void ClearSearchState()                      // Clear all search state
```

## Events

The service provides events for state change notifications:

```csharp
public event Action? OnUserSessionChanged;   // User login/logout
public event Action? OnEditingStateChanged;  // Edit mode changes
public event Action? OnPageStateChanged;     // Page state changes
```

## Usage Examples

### 1. Basic User Information Display

```razor
@inject UserSessionService SessionService

@if (SessionService.IsAuthenticated)
{
    <div class="user-info">
        <h4>Welcome, @SessionService.GetDisplayName()!</h4>
        <p>Role: @SessionService.GetRoleDisplayName()</p>
        
        @if (SessionService.IsAdmin())
        {
            <div class="admin-notice">You have admin privileges</div>
        }
    </div>
}
```

### 2. Editing State Management

```razor
@inject UserSessionService SessionService

@if (SessionService.IsEditing("project"))
{
    <div class="editing-indicator">
        <i class="fas fa-edit"></i>
        Editing: @SessionService.CurrentEditingProject?.Title
        <button @onclick="SessionService.ClearEditingState">Cancel</button>
    </div>
}

<button @onclick="StartEditing">Edit Project</button>

@code {
    private void StartEditing()
    {
        var project = new Project { Title = "My Project" };
        SessionService.StartEditingProject(project);
    }
}
```

### 3. Form Data Persistence

```razor
@inject UserSessionService SessionService

<EditForm Model="model" OnValidSubmit="HandleSubmit">
    <InputText @bind-Value="model.Title" 
               @oninput="(e) => SessionService.SetFormData('projectTitle', e.Value?.ToString())" />
    
    <button type="button" @onclick="SaveDraft">Save Draft</button>
    <button type="button" @onclick="LoadDraft">Load Draft</button>
</EditForm>

@code {
    private void SaveDraft()
    {
        SessionService.SetFormData("projectTitle", model.Title);
        SessionService.SetFormData("projectDescription", model.Description);
    }
    
    private void LoadDraft()
    {
        model.Title = SessionService.GetFormData("projectTitle") ?? "";
        model.Description = SessionService.GetFormData("projectDescription") ?? "";
    }
}
```

### 4. Page State Management

```razor
@inject UserSessionService SessionService

<div class="search-filters">
    <input @bind="searchQuery" @oninput="OnSearchChanged" placeholder="Search..." />
    <select @bind="skillFilter" @onchange="OnFilterChanged">
        <option value="">All Skills</option>
        <option value="Beginner">Beginner</option>
        <option value="Advanced">Advanced</option>
    </select>
</div>

@code {
    private string searchQuery = "";
    private string skillFilter = "";
    
    protected override void OnInitialized()
    {
        // Restore previous search state
        searchQuery = SessionService.LastSearchQuery ?? "";
        skillFilter = SessionService.SelectedSkillFilter ?? "";
    }
    
    private void OnSearchChanged(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";
        SessionService.SetSearchQuery(searchQuery);
    }
    
    private void OnFilterChanged(ChangeEventArgs e)
    {
        skillFilter = e.Value?.ToString() ?? "";
        SessionService.SetSkillFilter(skillFilter);
    }
}
```

### 5. Component State Subscription

```razor
@inject UserSessionService SessionService
@implements IDisposable

<div class="user-widget">
    @if (SessionService.IsAuthenticated)
    {
        <div class="avatar">@SessionService.GetUserInitials()</div>
        <span>@SessionService.GetDisplayName()</span>
        
        @if (SessionService.IsInEditMode)
        {
            <span class="edit-indicator">Editing @SessionService.CurrentEditingMode</span>
        }
    }
</div>

@code {
    protected override void OnInitialized()
    {
        // Subscribe to state changes for automatic UI updates
        SessionService.OnUserSessionChanged += StateHasChanged;
        SessionService.OnEditingStateChanged += StateHasChanged;
    }
    
    public void Dispose()
    {
        // Unsubscribe to prevent memory leaks
        SessionService.OnUserSessionChanged -= StateHasChanged;
        SessionService.OnEditingStateChanged -= StateHasChanged;
    }
}
```

## Integration with Authentication

The service automatically integrates with the `CustomAuthenticationStateProvider`:

- **Automatic Updates**: User data is automatically updated when authentication state changes
- **Claim Extraction**: Extracts user information from JWT claims
- **Logout Handling**: Automatically clears session data on logout
- **Role Mapping**: Maps JWT roles to user-friendly role names

## Best Practices

### 1. **Subscribe to State Changes**
Always subscribe to relevant events in components that display session data:

```csharp
protected override void OnInitialized()
{
    SessionService.OnUserSessionChanged += StateHasChanged;
}

public void Dispose()
{
    SessionService.OnUserSessionChanged -= StateHasChanged;
}
```

### 2. **Clear Temporary Data**
Clear form data and temporary state when operations complete:

```csharp
private async Task SaveProject()
{
    // Save the project
    await ProjectService.SaveAsync(project);
    
    // Clear editing state and form data
    SessionService.ClearEditingState();
    SessionService.ClearFormData();
}
```

### 3. **Use Page State for Complex UI**
Store complex UI state that should persist across navigation:

```csharp
// Store complex filter state
SessionService.SetPageState("projectFilters", new {
    Category = selectedCategory,
    DateRange = dateRange,
    SortOrder = sortOrder
});

// Retrieve and apply filters
var filters = SessionService.GetPageState<dynamic>("projectFilters");
if (filters != null)
{
    selectedCategory = filters.Category;
    dateRange = filters.DateRange;
    sortOrder = filters.SortOrder;
}
```

### 4. **Navigation Tracking**
Track user navigation for better UX:

```csharp
protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
    {
        SessionService.SetLastVisitedPage(Navigation.Uri);
    }
}
```

## Security Considerations

- **Client-Side Storage**: All data is stored in memory and is lost on page refresh
- **JWT Integration**: User identity is validated through JWT tokens
- **Role-Based Access**: Use role helpers for authorization decisions
- **Automatic Cleanup**: Session data is automatically cleared on logout

## Performance Benefits

- **Reduced API Calls**: User information cached in memory
- **State Persistence**: No need to refetch data between components
- **Efficient Updates**: Event-driven UI updates only when necessary
- **Memory Management**: Automatic cleanup prevents memory leaks

## Demo Page

The service includes a comprehensive demo page at `/session-demo` that demonstrates all features:
- User information display
- Editing state management
- Page state persistence
- Form data handling
- Search and filter state
- Event subscription patterns

Access the demo page after logging in to see the UserSessionService in action.
