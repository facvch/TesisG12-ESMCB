using BlazorFrontEnd.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class ReporteService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private const string BaseApiUrl = "https://localhost:7204/api/v1"; // Or configured BaseURL for exports

        public ReporteService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        // ════ METRICAS JSON ════

        public async Task<ReporteVentasDto?> GetReporteVentasAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            try
            {
                var qs = "";
                if (desde.HasValue) qs += $"?desde={desde.Value:yyyy-MM-dd}";
                if (hasta.HasValue) qs += (qs == "" ? "?" : "&") + $"hasta={hasta.Value:yyyy-MM-dd}";
                
                return await _httpClient.GetUnwrappedAsync<ReporteVentasDto>($"api/v1/Reporte/ventas{qs}");
            }
            catch { return null; }
        }

        public async Task<ReporteStockDto?> GetReporteStockAsync()
        {
            try { return await _httpClient.GetUnwrappedAsync<ReporteStockDto>("api/v1/Reporte/stock"); }
            catch { return null; }
        }

        public async Task<ReporteTurnosDto?> GetReporteTurnosAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            try
            {
                var qs = "";
                if (desde.HasValue) qs += $"?desde={desde.Value:yyyy-MM-dd}";
                if (hasta.HasValue) qs += (qs == "" ? "?" : "&") + $"hasta={hasta.Value:yyyy-MM-dd}";
                
                return await _httpClient.GetUnwrappedAsync<ReporteTurnosDto>($"api/v1/Reporte/turnos{qs}");
            }
            catch { return null; }
        }

        public async Task<ReporteClinicDto?> GetReporteClinicoAsync()
        {
            try { return await _httpClient.GetUnwrappedAsync<ReporteClinicDto>("api/v1/Reporte/clinico"); }
            catch { return null; }
        }

        // ════ EXPORTACIONES CSV (Browser Download) ════

        public async Task TriggerCsvDownloadAsync(string endpointUrl)
        {
            var absoluteUrl = $"{_httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7204"}/{endpointUrl.TrimStart('/')}";
            // Oculta la complejidad de descargar BLOB y fuerza al navegador a bajarse el CSV.
            await _jsRuntime.InvokeVoidAsync("open", absoluteUrl, "_blank");
        }
    }
}
