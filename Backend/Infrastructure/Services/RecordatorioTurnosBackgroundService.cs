using Application.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio en segundo plano que envía automáticamente un recordatorio por SMS
    /// un día antes del turno a los clientes de la veterinaria.
    /// </summary>
    public class RecordatorioTurnosBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecordatorioTurnosBackgroundService> _logger;
        private DateTime? _lastRunDate = null;

        public RecordatorioTurnosBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<RecordatorioTurnosBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RecordatorioTurnosBackgroundService] Iniciado.");

            // Esperar 1 minuto tras el inicio del servidor antes de la primera verificación
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool enableAutoSms = bool.TryParse(_configuration["Twilio:EnableAutomaticDailySms"], out var enabled) ? enabled : true;

                    var now = DateTime.Now;
                    // Ejecutar preferentemente por la mañana (entre las 08:00 y las 20:00) una vez al día
                    bool isDaytime = now.Hour >= 8 && now.Hour <= 20;
                    bool notRunToday = _lastRunDate == null || _lastRunDate.Value.Date != now.Date;

                    if (enableAutoSms && isDaytime && notRunToday)
                    {
                        _logger.LogInformation($"[RecordatorioTurnosBackgroundService] Procesando recordatorios automáticos por SMS para los turnos de mañana ({now.AddDays(1):dd/MM/yyyy})...");
                        await ProcesarTurnosDeMananaAsync();
                        _lastRunDate = now.Date;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RecordatorioTurnosBackgroundService] Error durante la ejecución del proceso diario de recordatorios.");
                }

                // Esperar 30 minutos antes de la siguiente verificación de ciclo
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

            _logger.LogInformation("[RecordatorioTurnosBackgroundService] Detenido.");
        }

        private async Task ProcesarTurnosDeMananaAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var turnoRepo = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();
            var smsService = scope.ServiceProvider.GetRequiredService<ITwilioSmsService>();

            var manana = DateTime.Today.AddDays(1);
            var turnos = await turnoRepo.GetByFechaAsync(manana);

            var turnosActivos = turnos
                .Where(t => t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Ausente)
                .ToList();

            if (!turnosActivos.Any())
            {
                _logger.LogInformation($"[RecordatorioTurnosBackgroundService] No hay turnos programados para mañana {manana:dd/MM/yyyy}.");
                return;
            }

            int enviados = 0;
            foreach (var t in turnosActivos)
            {
                var telefono = t.Paciente?.Propietario?.Telefono;
                if (string.IsNullOrWhiteSpace(telefono)) continue;

                var propNombre = t.Paciente?.Propietario?.Nombre ?? "Cliente";
                var pacNombre = t.Paciente?.Nombre ?? "tu mascota";
                var servNombre = t.Servicio?.Nombre ?? "Consulta";

                // Detección de cirugía o análisis para advertencia de ayuno
                bool isCirugiaOAnalisis = false;
                string motivoLower = (t.Motivo ?? "").ToLower();
                string servicioLower = (servNombre ?? "").ToLower();

                if (motivoLower.Contains("cirugia") || motivoLower.Contains("cirugía") ||
                    motivoLower.Contains("operacion") || motivoLower.Contains("operación") ||
                    motivoLower.Contains("quirurg") || motivoLower.Contains("quirúrg") ||
                    motivoLower.Contains("analisis") || motivoLower.Contains("análisis") ||
                    motivoLower.Contains("laboratorio") || motivoLower.Contains("sangre") ||
                    motivoLower.Contains("extraccion") || motivoLower.Contains("extracción") ||
                    servicioLower.Contains("cirugia") || servicioLower.Contains("cirugía") ||
                    servicioLower.Contains("analisis") || servicioLower.Contains("análisis") ||
                    servicioLower.Contains("laboratorio"))
                {
                    isCirugiaOAnalisis = true;
                }

                string msg = $"Hola {propNombre}, te recordamos que mañana {t.FechaHora:dd/MM} a las {t.FechaHora:HH:mm} hs tienes un turno en Veterinaria Ñandubay para {pacNombre} ({servNombre}).";
                if (isCirugiaOAnalisis)
                {
                    msg += $" Recuerde que {pacNombre} no debe ingerir alimentos por 24hs antes de su turno.";
                }

                var res = await smsService.SendSmsAsync(telefono, msg);
                if (res.Success)
                {
                    enviados++;
                    _logger.LogInformation($"[RecordatorioTurnosBackgroundService] SMS enviado a {telefono} para turno {t.Id}.");
                }
                else
                {
                    _logger.LogWarning($"[RecordatorioTurnosBackgroundService] No se pudo enviar SMS a {telefono}: {res.Message}");
                }
            }

            _logger.LogInformation($"[RecordatorioTurnosBackgroundService] Finalizado: {enviados}/{turnosActivos.Count} recordatorios SMS enviados para mañana {manana:dd/MM/yyyy}.");
        }
    }
}
