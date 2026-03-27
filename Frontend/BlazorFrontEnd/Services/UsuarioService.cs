using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/auth";

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UsuarioDto>?> GetAllUsersAsync()
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<List<UsuarioDto>>($"{BaseUrl}/usuarios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UsuarioService.GetAllUsers ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<List<RolDto>?> GetRolesAsync()
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<List<RolDto>>($"{BaseUrl}/roles");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UsuarioService.GetRoles ERROR] {ex.Message}");
                return new List<RolDto>();
            }
        }

        public async Task<bool> RegisterUserAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/register", request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UsuarioService.Register FAIL] Status: {(int)response.StatusCode} {response.StatusCode} - Body: {body}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/usuarios/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
