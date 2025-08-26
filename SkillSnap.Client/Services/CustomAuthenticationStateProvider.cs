using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SkillSnap.Client.Models.Dtos;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace SkillSnap.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";
        private bool _isInitialized = false;
        
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
                _isInitialized = true;
            }

            return new AuthenticationState(_currentUser);
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await SetTokenAsync(token);
            await SetAuthorizationHeaderAsync(token);
            
            // Parse the JWT token to extract claims
            var claims = ParseClaimsFromJwt(token);
            
            var identity = new ClaimsIdentity(claims, "jwt");
            _currentUser = new ClaimsPrincipal(identity);
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task MarkUserAsLoggedOut()
        {
            await RemoveTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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

        private async Task InitializeAsync()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                await SetAuthorizationHeaderAsync(token);
                
                // Parse the JWT token to extract claims
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                _currentUser = new ClaimsPrincipal(identity);
            }
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = new List<Claim>();

            foreach (var kvp in keyValuePairs!)
            {
                if (kvp.Value is JsonElement element)
                {
                    var value = element.ValueKind switch
                    {
                        JsonValueKind.String => element.GetString()!,
                        JsonValueKind.Number => element.GetInt64().ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => element.ToString()
                    };
                    
                    // Map standard JWT claims to .NET claim types
                    var claimType = kvp.Key switch
                    {
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => ClaimTypes.Email,
                        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                        _ => kvp.Key
                    };
                    
                    claims.Add(new Claim(claimType, value));
                }
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
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

        // Convenience methods for easier access
        public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;
        public string? CurrentUserName => _currentUser.Identity?.Name;
        public string? CurrentUserEmail => _currentUser.FindFirst(ClaimTypes.Email)?.Value;
        public string? CurrentUserRole => _currentUser.FindFirst(ClaimTypes.Role)?.Value;

        public bool IsInRole(string role)
        {
            return _currentUser.IsInRole(role);
        }

        public bool IsAdmin()
        {
            return IsInRole("Admin");
        }
    }
}
