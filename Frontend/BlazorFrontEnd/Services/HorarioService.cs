using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class HorarioService
    {
        private readonly HttpClient _httpClient;

        public HorarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TipoHorarioDto>> GetTiposHorarioAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<TipoHorarioDto>>("api/v1/tipohorario");
                return result ?? new List<TipoHorarioDto>();
            }
            catch
            {
                return new List<TipoHorarioDto>
                {
                    new TipoHorarioDto { Id = 1, Nombre = "Normal", Descripcion = "Horario regular" },
                    new TipoHorarioDto { Id = 2, Nombre = "Guardia", Descripcion = "Horario de guardia" }
                };
            }
        }

        public async Task<List<HorarioDto>> GetHorariosByVeterinarioAsync(string veterinarioId)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<HorarioDto>>($"api/v1/horario/veterinario/{veterinarioId}");
                return result ?? new List<HorarioDto>();
            }
            catch
            {
                return new List<HorarioDto>();
            }
        }
    }
}
