using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class VentaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/venta";

        public VentaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VentaDto>?> GetVentasAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            try
            {
                var qs = "";
                if (desde.HasValue) qs += $"?desde={desde.Value:yyyy-MM-dd}";
                if (hasta.HasValue) qs += (qs == "" ? "?" : "&") + $"hasta={hasta.Value:yyyy-MM-dd}";

                return await _httpClient.GetUnwrappedAsync<List<VentaDto>>($"{BaseUrl}{qs}");
            }
            catch { return null; }
        }

        public async Task<VentaDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<VentaDto>($"{BaseUrl}/{id}");
        }

        // Retorna el Id string de la venta creada
        public async Task<string?> CreateVentaAsync(CreateVentaRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
            if (response.IsSuccessStatusCode)
            {
                // El backend retorna: Created($"api/v1/Venta/{ventaId}", new { Id = ventaId, Total = venta.Total });
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                if (result != null && result.ContainsKey("id"))
                {
                    return result["id"].ToString();
                }
                return string.Empty; // Success pero no pudo parsear el Id
            }
            return null; // Error
        }

        public async Task<bool> AnularVentaAsync(string id, string motivo)
        {
            // El endpoint espera [FromBody] string motivo
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/anular", motivo);
            return response.IsSuccessStatusCode;
        }

        public async Task<FacturaDto?> FacturarVentaAsync(string id, FacturarRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{id}/facturar", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FacturaDto>();
            }
            return null;
        }
    }
}
