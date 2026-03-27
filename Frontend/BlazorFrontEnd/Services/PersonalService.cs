using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class PersonalService
    {
        private readonly HttpClient _httpClient;

        public PersonalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VeterinarioDto>?> GetVeterinariosActivosAsync()
        {
            try
            {
                var url = $"api/v1/Paginado/veterinarios?page=1&pageSize=1000";
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<VeterinarioDto>>(url);
                return res?.Items.Where(v => v.Activo).ToList() ?? new List<VeterinarioDto>();
            }
            catch
            {
                return new List<VeterinarioDto>();
            }
        }
    }
}

