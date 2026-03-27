using System.Text.Json.Serialization;

namespace BlazorFrontEnd.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        [JsonPropertyName("page")]
        public int PageIndex { get; set; }
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
        [JsonPropertyName("totalItems")]
        public int TotalCount { get; set; }
        [JsonPropertyName("hasPrevious")]
        public bool HasPreviousPage { get; set; }
        [JsonPropertyName("hasNext")]
        public bool HasNextPage { get; set; }
    }
}
