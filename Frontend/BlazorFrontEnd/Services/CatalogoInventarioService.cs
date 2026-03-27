using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class CatalogoInventarioService
    {
        private readonly HttpClient _httpClient;

        public CatalogoInventarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CategoriaDto>?> GetCategoriasAsync()
        {
            try
            {
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<CategoriaDto>>("api/v1/Categoria");
                return res?.Items.Where(c => c.Activo).ToList() ?? new List<CategoriaDto>();
            }
            catch { return new List<CategoriaDto>(); }
        }

        // Simulating the others since we are just scaffolding the catalog.
        // In reality these depend on the API being ready for Marcas, Proveedores, etc.
        public async Task<List<MarcaDto>?> GetMarcasAsync()
        {
            try { var res = await _httpClient.GetUnwrappedAsync<PaginatedList<MarcaDto>>("api/v1/Marca"); return res?.Items ?? new List<MarcaDto>(); }
            catch { return new List<MarcaDto>(); }
        }

        public async Task<List<ProveedorDto>?> GetProveedoresAsync()
        {
            try { var res = await _httpClient.GetUnwrappedAsync<PaginatedList<ProveedorDto>>("api/v1/Proveedor"); return res?.Items ?? new List<ProveedorDto>(); }
            catch { return new List<ProveedorDto>(); }
        }

        public async Task<List<DepositoDto>?> GetDepositosAsync()
        {
            try { var res = await _httpClient.GetUnwrappedAsync<PaginatedList<DepositoDto>>("api/v1/Deposito"); return res?.Items ?? new List<DepositoDto>(); }
            catch { return new List<DepositoDto>(); }
        }

        // --- Categoria CRUD ---
        public async Task<CategoriaDto?> GetCategoriaByIdAsync(int id) => await _httpClient.GetUnwrappedAsync<CategoriaDto>($"api/v1/Categoria/{id}");
        public async Task<(bool Success, string ErrorMessage)> CreateCategoriaAsync(CategoriaDto dto) 
        { 
            var res = await _httpClient.PostAsJsonAsync("api/v1/Categoria", dto); 
            if (res.IsSuccessStatusCode) return (true, "");
            return (false, await res.Content.ReadAsStringAsync()); 
        }
        public async Task<bool> UpdateCategoriaAsync(int id, CategoriaDto dto) { var res = await _httpClient.PutAsJsonAsync($"api/v1/Categoria", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> DeleteCategoriaAsync(int id) { var res = await _httpClient.DeleteAsync($"api/v1/Categoria/{id}"); return res.IsSuccessStatusCode; }

        // --- Marca CRUD ---
        public async Task<MarcaDto?> GetMarcaByIdAsync(int id) => await _httpClient.GetUnwrappedAsync<MarcaDto>($"api/v1/Marca/{id}");
        public async Task<bool> CreateMarcaAsync(MarcaDto dto) { var res = await _httpClient.PostAsJsonAsync("api/v1/Marca", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> UpdateMarcaAsync(int id, MarcaDto dto) { var res = await _httpClient.PutAsJsonAsync($"api/v1/Marca", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> DeleteMarcaAsync(int id) { var res = await _httpClient.DeleteAsync($"api/v1/Marca/{id}"); return res.IsSuccessStatusCode; }

        // --- Proveedor CRUD ---
        public async Task<ProveedorDto?> GetProveedorByIdAsync(string id) => await _httpClient.GetUnwrappedAsync<ProveedorDto>($"api/v1/Proveedor/{id}");
        public async Task<bool> CreateProveedorAsync(ProveedorDto dto) { var res = await _httpClient.PostAsJsonAsync("api/v1/Proveedor", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> UpdateProveedorAsync(string id, ProveedorDto dto) { var res = await _httpClient.PutAsJsonAsync($"api/v1/Proveedor", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> DeleteProveedorAsync(string id) { var res = await _httpClient.DeleteAsync($"api/v1/Proveedor/{id}"); return res.IsSuccessStatusCode; }

        // --- Deposito CRUD ---
        public async Task<DepositoDto?> GetDepositoByIdAsync(int id) => await _httpClient.GetUnwrappedAsync<DepositoDto>($"api/v1/Deposito/{id}");
        public async Task<bool> CreateDepositoAsync(DepositoDto dto) { var res = await _httpClient.PostAsJsonAsync("api/v1/Deposito", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> UpdateDepositoAsync(int id, DepositoDto dto) { var res = await _httpClient.PutAsJsonAsync($"api/v1/Deposito", dto); return res.IsSuccessStatusCode; }
        public async Task<bool> DeleteDepositoAsync(int id) { var res = await _httpClient.DeleteAsync($"api/v1/Deposito/{id}"); return res.IsSuccessStatusCode; }
    }
}
