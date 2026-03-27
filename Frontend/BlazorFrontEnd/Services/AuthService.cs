using BlazorFrontEnd.Models;
using System.Net.Http.Json;

namespace BlazorFrontEnd.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"[AUTH] Intentando login. Destino: {_httpClient.BaseAddress}api/v1/auth/login");
                var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[AUTH] Login exitoso HTTP 200");
                    // Read the wrapped response from the BaseController format
                    var wrappedResponse = await response.Content.ReadFromJsonAsync<LoginApiWrapper>();
                    return wrappedResponse?.Data;
                }
                
                Console.WriteLine($"[AUTH] Login falló con HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AUTH] Detalle del servidor: {errorBody}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTH EXCEPTION] Error catastrófico conectando a la API: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[AUTH EXCEPTION INNER] {ex.InnerException.Message}");
                }
                throw; // Rethrow para que el componente Login lo atrape
            }
        }
    }

    public class LoginApiWrapper
    {
        public bool Success { get; set; }
        public LoginResponse? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
