using Microsoft.JSInterop;
using SkillSnap.Client.Models.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkillSnap.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        
        // Convenience properties that delegate to the AuthenticationStateProvider
        public bool IsAuthenticated => _authStateProvider.IsAuthenticated;
        public string? CurrentUserName => _authStateProvider.CurrentUserName;
        public string? CurrentUserEmail => _authStateProvider.CurrentUserEmail;
        public string? CurrentUserRole => _authStateProvider.CurrentUserRole;

        public AuthService(HttpClient httpClient, CustomAuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResponse?.IsLoginSuccessful == true && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        // Update authentication state through the provider
                        await _authStateProvider.MarkUserAsAuthenticated(loginResponse.Token);
                        
                        return new AuthResult { IsSuccess = true, Message = "Login successful" };
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<AuthResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return errorResponse ?? new AuthResult { IsSuccess = false, Message = "Login failed" };
            }
            catch (Exception ex)
            {
                return new AuthResult { IsSuccess = false, Message = $"Login error: {ex.Message}" };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var registerResponse = JsonSerializer.Deserialize<AuthResult>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return registerResponse ?? new AuthResult { IsSuccess = true, Message = "Registration successful" };
                }

                var errorResponse = JsonSerializer.Deserialize<AuthResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return errorResponse ?? new AuthResult { IsSuccess = false, Message = "Registration failed" };
            }
            catch (Exception ex)
            {
                return new AuthResult { IsSuccess = false, Message = $"Registration error: {ex.Message}" };
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _authStateProvider.GetTokenAsync();
        }

        public bool IsInRole(string role)
        {
            return _authStateProvider.IsInRole(role);
        }

        public bool IsAdmin()
        {
            return _authStateProvider.IsAdmin();
        }
    }
}
