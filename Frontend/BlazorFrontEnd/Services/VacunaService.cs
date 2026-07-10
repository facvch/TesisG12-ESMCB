using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class VacunaService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/Vacuna";

        public VacunaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VacunaDto>?> GetAllAsync(bool soloActivas = false)
        {
            try
            {
                var result = await _httpClient.GetUnwrappedAsync<QueryResultWrapper<VacunaDto>>($"{BaseUrl}?soloActivas={soloActivas}");
                return result?.Items?.ToList() ?? new List<VacunaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VACUNA GET ALL ERROR] {ex.Message}");
                return new List<VacunaDto>();
            }
        }

        public async Task<bool> CreateAsync(VacunaDto modelo)
        {
            try
            {
                var request = new
                {
                    Nombre = modelo.Nombre,
                    Descripcion = modelo.Descripcion,
                    Laboratorio = modelo.Laboratorio,
                    IntervaloDosisDias = modelo.IntervaloDosisDias
                };
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VACUNA CREATE ERROR] {response.StatusCode}: {err}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VACUNA CREATE EXCEPTION] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(VacunaDto modelo)
        {
            try
            {
                var request = new
                {
                    Id = modelo.Id,
                    Nombre = modelo.Nombre,
                    Descripcion = modelo.Descripcion,
                    Laboratorio = modelo.Laboratorio,
                    IntervaloDosisDias = modelo.IntervaloDosisDias
                };
                var response = await _httpClient.PutAsJsonAsync(BaseUrl, request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
