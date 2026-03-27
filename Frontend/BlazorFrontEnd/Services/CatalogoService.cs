using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class CatalogoService
    {
        private readonly HttpClient _httpClient;

        public CatalogoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EspecieDto>?> GetEspeciesActivasAsync()
        {
            try
            {
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<EspecieDto>>("api/v1/Especie");
                return res?.Items ?? new List<EspecieDto>();
            }
            catch
            {
                return new List<EspecieDto>();
            }
        }

        public async Task<List<RazaDto>?> GetRazasPorEspecieAsync(int especieId)
        {
            try
            {
                // The API might return all Razas, and we filter on client, or we could pass query params. Let's get all and filter to be safe.
                var todas = await _httpClient.GetUnwrappedAsync<List<RazaDto>>("api/v1/Raza");
                return todas?.Where(r => r.EspecieId == especieId && r.Activo).ToList() ?? new List<RazaDto>();
            }
            catch
            {
                return new List<RazaDto>();
            }
        }
    }
}
