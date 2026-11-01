namespace api.Models;

public class PaymentSettings
{
    public int Id { get; set; }

    public string? AbaUrl { get; set; }

    public bool AbaPaymentEnabled { get; set; } = true;

    public byte[]? QrImageData { get; set; }

    public string? QrImageContentType { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
