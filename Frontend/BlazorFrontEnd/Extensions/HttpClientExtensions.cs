using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorFrontEnd.Extensions
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public static class HttpClientExtensions
    {
        public static async Task<T?> GetUnwrappedAsync<T>(this HttpClient client, string url)
        {
            try
            {
                var response = await client.GetFromJsonAsync<ApiResponse<T>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return response != null ? response.Data : default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTTP GET ERROR] {url}: {ex.Message}");
                return default;
            }
        }
    }
}
