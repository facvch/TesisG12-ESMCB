using BlazorFrontEnd.Models;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;

        public DashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<KpiDashboardDto?> GetKpisAsync()
        {
            return await _httpClient.GetUnwrappedAsync<KpiDashboardDto>("api/v1/Estadisticas/kpis");
        }

        public async Task<DashboardAlertasResponse?> GetAlertasDashboardAsync()
        {
            return await _httpClient.GetUnwrappedAsync<DashboardAlertasResponse>("api/v1/Recordatorio/dashboard");
        }

        public async Task<List<IngresoDiarioDto>?> GetIngresosDiariosAsync(int dias = 7)
        {
            var desde = DateTime.Today.AddDays(-dias).ToString("yyyy-MM-dd");
            var hasta = DateTime.Today.ToString("yyyy-MM-dd");
            return await _httpClient.GetUnwrappedAsync<List<IngresoDiarioDto>>($"api/v1/Estadisticas/ingresos/diario?desde={desde}&hasta={hasta}");
        }
    }
}
