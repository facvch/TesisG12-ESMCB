using BlazorFrontEnd.Models;
using BlazorFrontEnd.Extensions;
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

        /// <summary>
        /// Obtener perfil del usuario autenticado
        /// </summary>
        public async Task<UsuarioDto?> GetProfileAsync()
        {
            return await _httpClient.GetUnwrappedAsync<UsuarioDto>("api/v1/auth/me");
        }

        /// <summary>
        /// Actualizar datos del perfil (nombre completo, email)
        /// </summary>
        public async Task<(bool Success, string Error)> UpdateProfileAsync(UpdateProfileRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync("api/v1/auth/perfil", request);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }

        /// <summary>
        /// Subir/actualizar foto de perfil
        /// </summary>
        public async Task<(bool Success, string Error)> UpdatePhotoAsync(string? fotoBase64)
        {
            var response = await _httpClient.PutAsJsonAsync("api/v1/auth/perfil/foto", new UpdatePhotoRequest { FotoBase64 = fotoBase64 });
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }

        /// <summary>
        /// Cambiar contraseña
        /// </summary>
        public async Task<(bool Success, string Error)> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync("api/v1/auth/cambiarPassword", request);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }

        /// <summary>
        /// Obtener audit logs del usuario autenticado
        /// </summary>
        public async Task<List<AuditLogDto>?> GetMyAuditLogsAsync(int cantidad = 30)
        {
            return await _httpClient.GetUnwrappedAsync<List<AuditLogDto>>($"api/v1/auth/me/audit?cantidad={cantidad}");
        }

        /// <summary>
        /// Obtener datos de veterinario vinculado
        /// </summary>
        public async Task<VeterinarioPerfilDto?> GetMyVeterinarioAsync()
        {
            return await _httpClient.GetUnwrappedAsync<VeterinarioPerfilDto>("api/v1/auth/me/veterinario");
        }

        /// <summary>
        /// Guardar datos de veterinario vinculado
        /// </summary>
        public async Task<(bool Success, string Error)> SaveMyVeterinarioAsync(SaveVeterinarioRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync("api/v1/auth/me/veterinario", request);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }
    }

    public class LoginApiWrapper
    {
        public bool Success { get; set; }
        public LoginResponse? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
