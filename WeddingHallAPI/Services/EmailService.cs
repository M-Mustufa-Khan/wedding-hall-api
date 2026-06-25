using System.Net;
using System.Net.Mail;

namespace WeddingHallAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly bool _enabled;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config  = config;
            _logger  = logger;
            _enabled = config.GetValue<bool>("Email:Enabled");
        }

        public async Task SendBookingConfirmationAsync(string toEmail, string customerName, string hallName, DateTime eventDate, decimal totalPrice)
        {
            if (!_enabled) return;

            var subject = "Booking Received — Elegant Celebrations";
            var body = $@"
<html><body style=""font-family:Arial,sans-serif;background:#f9f9f9;padding:20px"">
  <div style=""max-width:560px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;border:1px solid #eee"">
    <div style=""background:#080808;padding:24px;text-align:center"">
      <p style=""color:#d4af37;letter-spacing:0.25em;font-size:0.65rem;margin:0;text-transform:uppercase"">ELEGANT</p>
      <h1 style=""color:#fff;margin:4px 0 0;font-size:1.3rem"">Celebrations</h1>
    </div>
    <div style=""padding:32px"">
      <h2 style=""color:#1a1a1a;margin-top:0"">Booking Received</h2>
      <p>Dear <strong>{customerName}</strong>,</p>
      <p>Thank you for your booking! Here are your reservation details:</p>
      <table style=""width:100%;border-collapse:collapse;margin:16px 0"">
        <tr><td style=""padding:8px;border-bottom:1px solid #eee;color:#555"">Hall</td>
            <td style=""padding:8px;border-bottom:1px solid #eee;font-weight:600"">{hallName}</td></tr>
        <tr><td style=""padding:8px;border-bottom:1px solid #eee;color:#555"">Event Date</td>
            <td style=""padding:8px;border-bottom:1px solid #eee;font-weight:600"">{eventDate:dd MMM yyyy}</td></tr>
        <tr><td style=""padding:8px;color:#555"">Total Amount</td>
            <td style=""padding:8px;font-weight:600;color:#d4af37"">Rs {totalPrice:N0}</td></tr>
      </table>
      <p style=""background:#fff8e1;border-left:4px solid #d4af37;padding:12px;border-radius:4px"">
        Your booking is currently <strong>Pending</strong>. Our team will review and confirm it within 24 hours.
      </p>
      <p style=""color:#666;font-size:0.88rem"">For any queries, call us at <strong>+92 318 225 5708</strong> or email <strong>info@elegantcelebrations.pk</strong></p>
    </div>
    <div style=""background:#f5f5f5;padding:16px;text-align:center;font-size:0.8rem;color:#999"">
      &copy; {DateTime.Now.Year} Elegant Celebrations — Nazimabad, Karachi
    </div>
  </div>
</body></html>";

            await SendAsync(toEmail, subject, body);
        }

        public async Task SendBookingStatusUpdateAsync(string toEmail, string customerName, string hallName, DateTime eventDate, string newStatus)
        {
            if (!_enabled) return;

            var subject = newStatus == "Confirmed"
                ? "Booking Confirmed ✓ — Elegant Celebrations"
                : $"Booking {newStatus} — Elegant Celebrations";

            var statusColor = newStatus == "Confirmed" ? "#4caf50" : "#ef4444";
            var body = $@"
<html><body style=""font-family:Arial,sans-serif;background:#f9f9f9;padding:20px"">
  <div style=""max-width:560px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;border:1px solid #eee"">
    <div style=""background:#080808;padding:24px;text-align:center"">
      <p style=""color:#d4af37;letter-spacing:0.25em;font-size:0.65rem;margin:0;text-transform:uppercase"">ELEGANT</p>
      <h1 style=""color:#fff;margin:4px 0 0;font-size:1.3rem"">Celebrations</h1>
    </div>
    <div style=""padding:32px"">
      <h2 style=""color:#1a1a1a;margin-top:0"">Booking {newStatus}</h2>
      <p>Dear <strong>{customerName}</strong>,</p>
      <p>Your booking status has been updated:</p>
      <p style=""font-size:1.1rem;font-weight:700;color:{statusColor}"">{newStatus}</p>
      <table style=""width:100%;border-collapse:collapse;margin:16px 0"">
        <tr><td style=""padding:8px;border-bottom:1px solid #eee;color:#555"">Hall</td>
            <td style=""padding:8px;border-bottom:1px solid #eee;font-weight:600"">{hallName}</td></tr>
        <tr><td style=""padding:8px;color:#555"">Event Date</td>
            <td style=""padding:8px;font-weight:600"">{eventDate:dd MMM yyyy}</td></tr>
      </table>
      <p style=""color:#666;font-size:0.88rem"">Questions? Call <strong>+92 318 225 5708</strong> or email <strong>info@elegantcelebrations.pk</strong></p>
    </div>
    <div style=""background:#f5f5f5;padding:16px;text-align:center;font-size:0.8rem;color:#999"">
      &copy; {DateTime.Now.Year} Elegant Celebrations — Nazimabad, Karachi
    </div>
  </div>
</body></html>";

            await SendAsync(toEmail, subject, body);
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var host     = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
                var port     = _config.GetValue<int>("Email:SmtpPort", 587);
                var sender   = _config["Email:SenderEmail"] ?? "";
                var senderName = _config["Email:SenderName"] ?? "Elegant Celebrations";
                var password = _config["Email:Password"] ?? "";

                using var client = new SmtpClient(host, port)
                {
                    Credentials    = new NetworkCredential(sender, password),
                    EnableSsl      = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                using var message = new MailMessage
                {
                    From       = new MailAddress(sender, senderName),
                    Subject    = subject,
                    Body       = htmlBody,
                    IsBodyHtml = true,
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Email send failed for {Email}: {Error}", toEmail, ex.Message);
            }
        }
    }
}
