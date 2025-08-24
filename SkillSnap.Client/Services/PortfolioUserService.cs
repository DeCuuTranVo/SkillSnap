using System.Net.Http.Json;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services;

public class PortfolioUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PortfolioUserService> _logger;
    private const string ApiEndpoint = "api/portfoliousers";

    public PortfolioUserService(HttpClient httpClient, ILogger<PortfolioUserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all portfolio users from the API
    /// </summary>
    /// <returns>List of portfolio users</returns>
    public async Task<List<PortfolioUser>> GetPortfolioUsersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all portfolio users from API");
            
            var portfolioUsers = await _httpClient.GetFromJsonAsync<List<PortfolioUser>>(ApiEndpoint);
            
            _logger.LogInformation("Successfully retrieved {Count} portfolio users", portfolioUsers?.Count ?? 0);
            return portfolioUsers ?? new List<PortfolioUser>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching portfolio users");
            throw new InvalidOperationException("Failed to retrieve portfolio users from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching portfolio users");
            throw;
        }
    }
    
    /// <summary>
    /// Get the first portfolio user from the API
    /// </summary>
    /// <returns>The first portfolio user or null if none exist</returns>
    public async Task<PortfolioUser?> GetFirstPortfolioUserAsync()
    {
        try
        {
            var users = await GetPortfolioUsersAsync();
            return users.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching the first portfolio user");
            throw new InvalidOperationException("Failed to retrieve the first portfolio user", ex);
        }
    }

    /// <summary>
    /// Get a specific portfolio user by ID
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>Portfolio user details or null if not found</returns>
    public async Task<PortfolioUser?> GetPortfolioUserByIdAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Fetching portfolio user with ID {PortfolioUserId}", portfolioUserId);

            var portfolioUser = await _httpClient.GetFromJsonAsync<PortfolioUser>($"{ApiEndpoint}/{portfolioUserId}");

            _logger.LogInformation("Successfully retrieved portfolio user: {UserName}", portfolioUser?.Name ?? "Unknown");
            return portfolioUser;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Portfolio user with ID {PortfolioUserId} not found", portfolioUserId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching portfolio user {PortfolioUserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to retrieve portfolio user {portfolioUserId} from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching portfolio user {PortfolioUserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Get a portfolio user with their projects included
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>Portfolio user with projects or null if not found</returns>
    public async Task<PortfolioUser?> GetPortfolioUserWithProjectsAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Fetching portfolio user with projects for ID {PortfolioUserId}", portfolioUserId);
            
            var portfolioUser = await _httpClient.GetFromJsonAsync<PortfolioUser>($"{ApiEndpoint}/{portfolioUserId}/projects");
            
            _logger.LogInformation("Successfully retrieved portfolio user with {ProjectCount} projects: {UserName}", 
                portfolioUser?.Projects?.Count ?? 0, portfolioUser?.Name ?? "Unknown");
            return portfolioUser;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Portfolio user with ID {PortfolioUserId} not found", portfolioUserId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching portfolio user with projects {PortfolioUserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to retrieve portfolio user with projects {portfolioUserId} from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching portfolio user with projects {PortfolioUserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Get a portfolio user with their skills included
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>Portfolio user with skills or null if not found</returns>
    public async Task<PortfolioUser?> GetPortfolioUserWithSkillsAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Fetching portfolio user with skills for ID {PortfolioUserId}", portfolioUserId);
            
            var portfolioUser = await _httpClient.GetFromJsonAsync<PortfolioUser>($"{ApiEndpoint}/{portfolioUserId}/skills");
            
            _logger.LogInformation("Successfully retrieved portfolio user with {SkillCount} skills: {UserName}", 
                portfolioUser?.Skills?.Count ?? 0, portfolioUser?.Name ?? "Unknown");
            return portfolioUser;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Portfolio user with ID {PortfolioUserId} not found", portfolioUserId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching portfolio user with skills {PortfolioUserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to retrieve portfolio user with skills {portfolioUserId} from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching portfolio user with skills {PortfolioUserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Add a new portfolio user
    /// </summary>
    /// <param name="newPortfolioUser">Portfolio user to create</param>
    /// <returns>Created portfolio user with assigned ID</returns>
    public async Task<PortfolioUser> AddPortfolioUserAsync(PortfolioUser newPortfolioUser)
    {
        try
        {
            _logger.LogInformation("Creating new portfolio user: {UserName}", newPortfolioUser.Name);
            
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, newPortfolioUser);
            
            if (response.IsSuccessStatusCode)
            {
                var createdPortfolioUser = await response.Content.ReadFromJsonAsync<PortfolioUser>();
                
                _logger.LogInformation("Successfully created portfolio user with ID {PortfolioUserId}", createdPortfolioUser?.Id);
                return createdPortfolioUser ?? throw new InvalidOperationException("Failed to deserialize created portfolio user");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create portfolio user. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to create portfolio user: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating portfolio user");
            throw new InvalidOperationException("Failed to create portfolio user due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating portfolio user");
            throw;
        }
    }

    /// <summary>
    /// Update an existing portfolio user
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID to update</param>
    /// <param name="updatedPortfolioUser">Updated portfolio user data</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdatePortfolioUserAsync(int portfolioUserId, PortfolioUser updatedPortfolioUser)
    {
        try
        {
            _logger.LogInformation("Updating portfolio user {PortfolioUserId}: {UserName}", portfolioUserId, updatedPortfolioUser.Name);
            
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{portfolioUserId}", updatedPortfolioUser);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated portfolio user {PortfolioUserId}", portfolioUserId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update portfolio user {PortfolioUserId}. Status: {StatusCode}, Error: {Error}", 
                    portfolioUserId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while updating portfolio user {PortfolioUserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to update portfolio user {portfolioUserId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating portfolio user {PortfolioUserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Delete a portfolio user
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID to delete</param>
    /// <returns>True if successful</returns>
    public async Task<bool> DeletePortfolioUserAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Deleting portfolio user {PortfolioUserId}", portfolioUserId);
            
            var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{portfolioUserId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted portfolio user {PortfolioUserId}", portfolioUserId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete portfolio user {PortfolioUserId}. Status: {StatusCode}, Error: {Error}", 
                    portfolioUserId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while deleting portfolio user {PortfolioUserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to delete portfolio user {portfolioUserId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting portfolio user {PortfolioUserId}", portfolioUserId);
            throw;
        }
    }
}
