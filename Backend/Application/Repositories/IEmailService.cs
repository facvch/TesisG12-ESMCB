using Application.DataTransferObjects;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IEmailService
    {
        Task<EmailResponseDto> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainText = null);
        Task<EmailResponseDto> SendTurnoConfirmacionAsync(Turno turno, Paciente paciente, Propietario propietario, Veterinario veterinario, Servicio servicio, Sucursal? sucursal);
        string BuildGoogleCalendarUrl(string titulo, DateTime fechaHora, int duracionMinutos, string descripcion, string ubicacion);
    }
}
