using System.Net.Http.Json;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services;

public class ProjectService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectService> _logger;
    private const string ApiEndpoint = "api/projects";

    public ProjectService(HttpClient httpClient, ILogger<ProjectService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects from the API
    /// </summary>
    /// <returns>List of projects</returns>
    public async Task<List<Project>> GetProjectsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all projects from API");
            
            var projects = await _httpClient.GetFromJsonAsync<List<Project>>(ApiEndpoint);
            
            _logger.LogInformation("Successfully retrieved {Count} projects", projects?.Count ?? 0);
            return projects ?? new List<Project>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching projects");
            throw new InvalidOperationException("Failed to retrieve projects from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching projects");
            throw;
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Project details or null if not found</returns>
    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Fetching project with ID {ProjectId}", projectId);
            
            var project = await _httpClient.GetFromJsonAsync<Project>($"{ApiEndpoint}/{projectId}");
            
            _logger.LogInformation("Successfully retrieved project: {ProjectTitle}", project?.Title ?? "Unknown");
            return project;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Project with ID {ProjectId} not found", projectId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to retrieve project {projectId} from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Get projects for a specific portfolio user
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>List of projects for the user</returns>
    public async Task<List<Project>> GetProjectsByUserAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Fetching projects for user {UserId}", portfolioUserId);
            
            var projects = await _httpClient.GetFromJsonAsync<List<Project>>($"{ApiEndpoint}/user/{portfolioUserId}");
            
            _logger.LogInformation("Successfully retrieved {Count} projects for user {UserId}", 
                projects?.Count ?? 0, portfolioUserId);
            return projects ?? new List<Project>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("User with ID {UserId} not found", portfolioUserId);
            return new List<Project>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching projects for user {UserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to retrieve projects for user {portfolioUserId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching projects for user {UserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Add a new project
    /// </summary>
    /// <param name="newProject">Project to create</param>
    /// <returns>Created project with assigned ID</returns>
    public async Task<Project> AddProjectAsync(Project newProject)
    {
        try
        {
            _logger.LogInformation("Creating new project: {ProjectTitle}", newProject.Title);
            
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, newProject);
            
            if (response.IsSuccessStatusCode)
            {
                var createdProject = await response.Content.ReadFromJsonAsync<Project>();
                
                _logger.LogInformation("Successfully created project with ID {ProjectId}", createdProject?.Id);
                return createdProject ?? throw new InvalidOperationException("Failed to deserialize created project");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create project. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to create project: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating project");
            throw new InvalidOperationException("Failed to create project due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating project");
            throw;
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="projectId">Project ID to update</param>
    /// <param name="updatedProject">Updated project data</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdateProjectAsync(int projectId, Project updatedProject)
    {
        try
        {
            _logger.LogInformation("Updating project {ProjectId}: {ProjectTitle}", projectId, updatedProject.Title);
            
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{projectId}", updatedProject);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated project {ProjectId}", projectId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update project {ProjectId}. Status: {StatusCode}, Error: {Error}", 
                    projectId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while updating project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to update project {projectId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="projectId">Project ID to delete</param>
    /// <returns>True if successful</returns>
    public async Task<bool> DeleteProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Deleting project {ProjectId}", projectId);
            
            var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{projectId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted project {ProjectId}", projectId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete project {ProjectId}. Status: {StatusCode}, Error: {Error}", 
                    projectId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while deleting project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to delete project {projectId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting project {ProjectId}", projectId);
            throw;
        }
    }
}
