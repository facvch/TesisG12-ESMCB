using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class MetodoPagoService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/metodopago";

        public MetodoPagoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MetodoPagoDto>?> GetAllActivosAsync()
        {
            try
            {
                // El endpoint retorna un QueryResult<MetodoPagoDto>
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<MetodoPagoDto>>($"{BaseUrl}?soloActivos=true");
                return res?.Items ?? new List<MetodoPagoDto>();
            }
            catch { return null; }
        }
    }
}
