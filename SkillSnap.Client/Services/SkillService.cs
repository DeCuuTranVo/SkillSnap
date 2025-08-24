using System.Net.Http.Json;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services;

public class SkillService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SkillService> _logger;
    private const string ApiEndpoint = "api/skills";

    public SkillService(HttpClient httpClient, ILogger<SkillService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all skills from the API
    /// </summary>
    /// <returns>List of skills</returns>
    public async Task<List<Skill>> GetSkillsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all skills from API");
            
            var skills = await _httpClient.GetFromJsonAsync<List<Skill>>(ApiEndpoint);
            
            _logger.LogInformation("Successfully retrieved {Count} skills", skills?.Count ?? 0);
            return skills ?? new List<Skill>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching skills");
            throw new InvalidOperationException("Failed to retrieve skills from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching skills");
            throw;
        }
    }

    /// <summary>
    /// Get a specific skill by ID
    /// </summary>
    /// <param name="skillId">Skill ID</param>
    /// <returns>Skill details or null if not found</returns>
    public async Task<Skill?> GetSkillByIdAsync(int skillId)
    {
        try
        {
            _logger.LogInformation("Fetching skill with ID {SkillId}", skillId);
            
            var skill = await _httpClient.GetFromJsonAsync<Skill>($"{ApiEndpoint}/{skillId}");
            
            _logger.LogInformation("Successfully retrieved skill: {SkillName}", skill?.Name ?? "Unknown");
            return skill;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Skill with ID {SkillId} not found", skillId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching skill {SkillId}", skillId);
            throw new InvalidOperationException($"Failed to retrieve skill {skillId} from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching skill {SkillId}", skillId);
            throw;
        }
    }

    /// <summary>
    /// Get skills for a specific portfolio user
    /// </summary>
    /// <param name="portfolioUserId">Portfolio User ID</param>
    /// <returns>List of skills for the user</returns>
    public async Task<List<Skill>> GetSkillsByUserAsync(int portfolioUserId)
    {
        try
        {
            _logger.LogInformation("Fetching skills for user {UserId}", portfolioUserId);
            
            var skills = await _httpClient.GetFromJsonAsync<List<Skill>>($"{ApiEndpoint}/user/{portfolioUserId}");
            
            _logger.LogInformation("Successfully retrieved {Count} skills for user {UserId}", 
                skills?.Count ?? 0, portfolioUserId);
            return skills ?? new List<Skill>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("User with ID {UserId} not found", portfolioUserId);
            return new List<Skill>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching skills for user {UserId}", portfolioUserId);
            throw new InvalidOperationException($"Failed to retrieve skills for user {portfolioUserId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching skills for user {UserId}", portfolioUserId);
            throw;
        }
    }

    /// <summary>
    /// Get skills by proficiency level
    /// </summary>
    /// <param name="level">Skill level (Beginner, Intermediate, Advanced, Professional)</param>
    /// <returns>List of skills with the specified level</returns>
    public async Task<List<Skill>> GetSkillsByLevelAsync(string level)
    {
        try
        {
            _logger.LogInformation("Fetching skills with level {Level}", level);
            
            var skills = await _httpClient.GetFromJsonAsync<List<Skill>>($"{ApiEndpoint}/level/{level}");
            
            _logger.LogInformation("Successfully retrieved {Count} skills with level {Level}", 
                skills?.Count ?? 0, level);
            return skills ?? new List<Skill>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching skills with level {Level}", level);
            throw new InvalidOperationException($"Failed to retrieve skills with level {level}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching skills with level {Level}", level);
            throw;
        }
    }

    /// <summary>
    /// Get available skill levels
    /// </summary>
    /// <returns>List of valid skill levels</returns>
    public async Task<List<string>> GetSkillLevelsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching available skill levels");
            
            var levels = await _httpClient.GetFromJsonAsync<List<string>>($"{ApiEndpoint}/levels");
            
            _logger.LogInformation("Successfully retrieved {Count} skill levels", levels?.Count ?? 0);
            return levels ?? new List<string>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching skill levels");
            throw new InvalidOperationException("Failed to retrieve skill levels from the server", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching skill levels");
            throw;
        }
    }

    /// <summary>
    /// Add a new skill
    /// </summary>
    /// <param name="newSkill">Skill to create</param>
    /// <returns>Created skill with assigned ID</returns>
    public async Task<Skill> AddSkillAsync(Skill newSkill)
    {
        try
        {
            _logger.LogInformation("Creating new skill: {SkillName} ({Level})", newSkill.Name, newSkill.Level);
            
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, newSkill);
            
            if (response.IsSuccessStatusCode)
            {
                var createdSkill = await response.Content.ReadFromJsonAsync<Skill>();
                
                _logger.LogInformation("Successfully created skill with ID {SkillId}", createdSkill?.Id);
                return createdSkill ?? throw new InvalidOperationException("Failed to deserialize created skill");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create skill. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to create skill: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating skill");
            throw new InvalidOperationException("Failed to create skill due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating skill");
            throw;
        }
    }

    /// <summary>
    /// Update an existing skill
    /// </summary>
    /// <param name="skillId">Skill ID to update</param>
    /// <param name="updatedSkill">Updated skill data</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdateSkillAsync(int skillId, Skill updatedSkill)
    {
        try
        {
            _logger.LogInformation("Updating skill {SkillId}: {SkillName} ({Level})", 
                skillId, updatedSkill.Name, updatedSkill.Level);
            
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{skillId}", updatedSkill);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated skill {SkillId}", skillId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update skill {SkillId}. Status: {StatusCode}, Error: {Error}", 
                    skillId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while updating skill {SkillId}", skillId);
            throw new InvalidOperationException($"Failed to update skill {skillId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating skill {SkillId}", skillId);
            throw;
        }
    }

    /// <summary>
    /// Delete a skill
    /// </summary>
    /// <param name="skillId">Skill ID to delete</param>
    /// <returns>True if successful</returns>
    public async Task<bool> DeleteSkillAsync(int skillId)
    {
        try
        {
            _logger.LogInformation("Deleting skill {SkillId}", skillId);
            
            var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{skillId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted skill {SkillId}", skillId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete skill {SkillId}. Status: {StatusCode}, Error: {Error}", 
                    skillId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while deleting skill {SkillId}", skillId);
            throw new InvalidOperationException($"Failed to delete skill {skillId} due to network error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting skill {SkillId}", skillId);
            throw;
        }
    }
}
