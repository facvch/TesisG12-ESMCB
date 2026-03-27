using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class CatalogoServiciosService
    {
        private readonly HttpClient _httpClient;

        public CatalogoServiciosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ServicioDto>?> GetServiciosActivosAsync()
        {
            try
            {
                var url = $"api/v1/Paginado/servicios?page=1&pageSize=1000";
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<ServicioDto>>(url);
                return res?.Items.Where(s => s.Activo).ToList() ?? new List<ServicioDto>();
            }
            catch
            {
                return new List<ServicioDto>();
            }
        }
    }
}

