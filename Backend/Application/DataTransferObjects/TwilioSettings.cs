namespace Application.DataTransferObjects
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhoneNumber { get; set; } = "whatsapp:+NUMERO_AQUI";
        public string SmsFromPhoneNumber { get; set; } = "+sms_NUMERO_AQUI";
        public string DefaultCountryCode { get; set; } = "+549";
        public bool EnableTwilio { get; set; } = true;
        public bool EnableAutomaticDailySms { get; set; } = true;
    }

    public class EmailSettings
    {
        public string SendGridApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "notificaciones@veterinarianandubay.com";
        public string FromName { get; set; } = "Veterinaria Ñandubay";
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string SafeTestEmail { get; set; } = string.Empty;
        public bool EnableSafeMode { get; set; } = true;
        public bool EnableEmail { get; set; } = true;
    }

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

    // ── Email ──
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

    // ── Estado Global de Notificaciones ──
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
