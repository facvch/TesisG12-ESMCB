namespace BlazorFrontEnd.Models
{
    // ── Twilio SMS ──
    public class SendSmsRequest
    {
        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }

    public class SmsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MessageSid { get; set; }
        public string? FormattedPhone { get; set; }
        public string? Status { get; set; }
    }

    // ── Email & Google Calendar ──
    public class SendEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
    }

    public class EmailResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RecipientEmail { get; set; }
        public string? Subject { get; set; }
        public string? GoogleCalendarUrl { get; set; }
    }

    // ── Twilio WhatsApp (compatibilidad) ──
    public class SendWhatsAppRequest
    {
        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }

    public class WhatsAppResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MessageSid { get; set; }
        public string? FormattedPhone { get; set; }
        public string? Status { get; set; }
    }

    public class TwilioStatusDto
    {
        public bool IsConfigured { get; set; }
        public bool EnableTwilio { get; set; }
        public string FromPhoneNumber { get; set; } = string.Empty;
        public string SmsFromPhoneNumber { get; set; } = string.Empty;
        public string DefaultCountryCode { get; set; } = string.Empty;
        public bool EmailConfigured { get; set; }
        public string SafeTestEmail { get; set; } = string.Empty;
    }
}
