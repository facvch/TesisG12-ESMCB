using System.Collections.Concurrent;

namespace API.Middleware
{
    /// <summary>
    /// Middleware que mide el tiempo de respuesta de cada request
    /// y acumula métricas de performance por endpoint.
    /// </summary>
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, EndpointMetrics> _metrics = new();
        private static DateTime _startTime = DateTime.UtcNow;

        public PerformanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var path = NormalizePath(context.Request.Path.ToString());
            var method = context.Request.Method;

            // Registrar callback para headers ANTES de que se envíe la respuesta
            context.Response.OnStarting(() =>
            {
                sw.Stop();
                var ms = sw.ElapsedMilliseconds;
                try { context.Response.Headers["X-Response-Time"] = $"{ms}ms"; } catch { }

                // Registrar métrica
                var key = $"{method} {path}";
                var metrics = _metrics.GetOrAdd(key, _ => new EndpointMetrics { Endpoint = key });
                lock (metrics)
                {
                    metrics.TotalRequests++;
                    metrics.TotalMs += ms;
                    if (ms > metrics.MaxMs) metrics.MaxMs = ms;
                    if (ms < metrics.MinMs || metrics.MinMs == 0) metrics.MinMs = ms;
                    metrics.LastMs = ms;
                    metrics.LastAccess = DateTime.UtcNow;
                    metrics.RecentTimes.Enqueue(ms);
                    if (metrics.RecentTimes.Count > 100)
                        metrics.RecentTimes.TryDequeue(out _);
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private static string NormalizePath(string path)
        {
            // Reemplazar IDs con {id} para agrupar
            var parts = path.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (Guid.TryParse(parts[i], out _) || (parts[i].Length > 8 && parts[i].All(c => char.IsLetterOrDigit(c) || c == '-')))
                {
                    // Heurística: si parece un GUID/ID largo, reemplazar
                    if (i > 0 && parts[i].Length >= 20)
                        parts[i] = "{id}";
                }
                else if (int.TryParse(parts[i], out _))
                {
                    parts[i] = "{id}";
                }
            }
            return string.Join("/", parts);
        }

        // API pública para el controller
        public static PerformanceSummary GetSummary()
        {
            var uptime = DateTime.UtcNow - _startTime;
            var allMetrics = _metrics.Values.ToList();

            return new PerformanceSummary
            {
                UptimeSeconds = (int)uptime.TotalSeconds,
                Uptime = FormatUptime(uptime),
                TotalRequests = allMetrics.Sum(m => m.TotalRequests),
                RequestsPerSecond = uptime.TotalSeconds > 0
                    ? Math.Round(allMetrics.Sum(m => m.TotalRequests) / uptime.TotalSeconds, 2)
                    : 0,
                AvgResponseMs = allMetrics.Sum(m => m.TotalRequests) > 0
                    ? Math.Round((double)allMetrics.Sum(m => m.TotalMs) / allMetrics.Sum(m => m.TotalRequests), 1)
                    : 0,
                Endpoints = allMetrics
                    .OrderByDescending(m => m.TotalRequests)
                    .Select(m =>
                    {
                        long[] sorted;
                        lock (m) { sorted = m.RecentTimes.OrderBy(x => x).ToArray(); }
                        return new EndpointPerf
                        {
                            Endpoint = m.Endpoint,
                            Requests = m.TotalRequests,
                            AvgMs = m.TotalRequests > 0 ? Math.Round((double)m.TotalMs / m.TotalRequests, 1) : 0,
                            MinMs = m.MinMs,
                            MaxMs = m.MaxMs,
                            P95Ms = sorted.Length > 0 ? sorted[(int)(sorted.Length * 0.95)] : 0,
                            P99Ms = sorted.Length > 0 ? sorted[(int)(sorted.Length * 0.99)] : 0,
                            LastMs = m.LastMs
                        };
                    }).ToList()
            };
        }

        public static void Reset()
        {
            _metrics.Clear();
            _startTime = DateTime.UtcNow;
        }

        private static string FormatUptime(TimeSpan ts)
        {
            if (ts.TotalDays >= 1) return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
        }
    }

    // Modelos
    internal class EndpointMetrics
    {
        public string Endpoint { get; set; }
        public long TotalRequests { get; set; }
        public long TotalMs { get; set; }
        public long MaxMs { get; set; }
        public long MinMs { get; set; }
        public long LastMs { get; set; }
        public DateTime LastAccess { get; set; }
        public ConcurrentQueue<long> RecentTimes { get; } = new();
    }

    public class PerformanceSummary
    {
        public int UptimeSeconds { get; set; }
        public string Uptime { get; set; }
        public long TotalRequests { get; set; }
        public double RequestsPerSecond { get; set; }
        public double AvgResponseMs { get; set; }
        public List<EndpointPerf> Endpoints { get; set; }
    }

    public class EndpointPerf
    {
        public string Endpoint { get; set; }
        public long Requests { get; set; }
        public double AvgMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public long P95Ms { get; set; }
        public long P99Ms { get; set; }
        public long LastMs { get; set; }
    }
}
