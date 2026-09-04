using Application.DataTransferObjects;

namespace Application.Repositories
{
    public interface ITwilioSmsService
    {
        Task<SmsResponseDto> SendSmsAsync(string telefono, string mensaje);
        Task<TwilioStatusDto> GetStatusAsync();
    }
}
