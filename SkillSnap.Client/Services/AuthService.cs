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
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";
        private const string UserKey = "currentUser";
        private bool _isInitialized = false;

        public event Action<bool>? AuthStateChanged;
        public bool IsAuthenticated { get; private set; }
        public string? CurrentUserName { get; private set; }
        public string? CurrentUserEmail { get; private set; }
        public string? CurrentUserRole { get; private set; }

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return; // Prevent multiple initializations
            
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                await SetAuthorizationHeaderAsync(token);
                await LoadUserInfoAsync();
            }
            
            _isInitialized = true;
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
                        await SetTokenAsync(loginResponse.Token);
                        await SetAuthorizationHeaderAsync(loginResponse.Token);
                        await SetUserInfoAsync(loginResponse.UserName, loginResponse.Email, loginResponse.Role);
                        
                        IsAuthenticated = true;
                        CurrentUserName = loginResponse.UserName;
                        CurrentUserEmail = loginResponse.Email;
                        CurrentUserRole = loginResponse.Role;
                        
                        AuthStateChanged?.Invoke(true);
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
            await RemoveTokenAsync();
            await RemoveUserInfoAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            IsAuthenticated = false;
            CurrentUserName = null;
            CurrentUserEmail = null;
            CurrentUserRole = null;
            
            AuthStateChanged?.Invoke(false);
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
            }
            catch
            {
                return null;
            }
        }

        private async Task SetTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }

        private async Task RemoveTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }

        private Task SetAuthorizationHeaderAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return Task.CompletedTask;
        }

        private async Task SetUserInfoAsync(string userName, string email, string role)
        {
            var userInfo = new { UserName = userName, Email = email, Role = role };
            var json = JsonSerializer.Serialize(userInfo);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, json);
        }

        private async Task LoadUserInfoAsync()
        {
            try
            {
                var userInfoJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserKey);
                if (!string.IsNullOrEmpty(userInfoJson))
                {
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (userInfo != null)
                    {
                        IsAuthenticated = true;
                        CurrentUserName = userInfo.UserName;
                        CurrentUserEmail = userInfo.Email;
                        CurrentUserRole = userInfo.Role;
                        AuthStateChanged?.Invoke(true);
                    }
                }
            }
            catch
            {
                // If there's an error loading user info, clear everything
                await LogoutAsync();
            }
        }

        private async Task RemoveUserInfoAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
        }

        public bool IsInRole(string role)
        {
            return CurrentUserRole?.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
        }

        public bool IsAdmin()
        {
            return IsInRole("Admin");
        }
    }
}
