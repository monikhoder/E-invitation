using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/payment-settings")]
public class PaymentSettingsController : ControllerBase
{
    private const int SettingsId = 1;
    private const long MaximumQrFileSize = 5 * 1024 * 1024;
    private const string DefaultQrImageUrl = "/images/aba%20QR.png";
    private readonly AppDbContext _db;

    public PaymentSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var settings = await _db.PaymentSettings
            .AsNoTracking()
            .Where(x => x.Id == SettingsId)
            .Select(x => new
            {
                x.AbaUrl,
                x.AbaPaymentEnabled,
                HasQrImage = x.QrImageData != null,
                x.UpdatedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        return Ok(new
        {
            abaPaymentEnabled = settings?.AbaPaymentEnabled ?? true,
            abaUrl = settings?.AbaUrl,
            qrImageUrl = GetQrImageUrl(settings?.HasQrImage == true, settings?.UpdatedAtUtc),
        });
    }

    [AllowAnonymous]
    [HttpGet("qr")]
    public async Task<IActionResult> GetQrImage(CancellationToken cancellationToken)
    {
        var settings = await _db.PaymentSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (settings?.QrImageData is not { Length: > 0 } ||
            settings.QrImageContentType is null)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "no-cache";
        return File(settings.QrImageData, settings.QrImageContentType);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var settings = await _db.PaymentSettings
            .AsNoTracking()
            .Where(x => x.Id == SettingsId)
            .Select(x => new
            {
                x.AbaUrl,
                x.AbaPaymentEnabled,
                HasQrImage = x.QrImageData != null,
                x.UpdatedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        return Ok(new
        {
            abaPaymentEnabled = settings?.AbaPaymentEnabled ?? true,
            abaUrl = settings?.AbaUrl,
            qrImageUrl = GetQrImageUrl(settings?.HasQrImage == true, settings?.UpdatedAtUtc),
            hasUploadedQr = settings?.HasQrImage == true,
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    [RequestSizeLimit(MaximumQrFileSize + 64 * 1024)]
    public async Task<IActionResult> Update(
        [FromForm] UpdatePaymentSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var abaUrl = request.AbaUrl?.Trim();
        if (request.AbaPaymentEnabled is not bool abaPaymentEnabled)
        {
            return BadRequest(new
            {
                message = "ABA payment enabled setting is required."
            });
        }

        if (!string.IsNullOrEmpty(abaUrl) &&
            (!Uri.TryCreate(abaUrl, UriKind.Absolute, out var paymentUri) ||
             paymentUri.Scheme != Uri.UriSchemeHttps ||
             string.IsNullOrWhiteSpace(paymentUri.Host)))
        {
            return BadRequest(new
            {
                message = "ABA payment URL must be a valid HTTPS URL."
            });
        }

        byte[]? qrImageData = null;
        string? qrImageContentType = null;
        if (request.QrImage is not null)
        {
            if (request.QrImage.Length == 0 || request.QrImage.Length > MaximumQrFileSize)
            {
                return BadRequest(new
                {
                    message = "QR image must be between 1 byte and 5 MB."
                });
            }

            await using var memoryStream = new MemoryStream();
            await request.QrImage.CopyToAsync(memoryStream, cancellationToken);
            qrImageData = memoryStream.ToArray();
            qrImageContentType = DetectImageContentType(qrImageData);
            if (qrImageContentType is null)
            {
                return BadRequest(new
                {
                    message = "Upload a PNG, JPEG, or WebP QR image."
                });
            }
        }

        var settings = await _db.PaymentSettings
            .SingleOrDefaultAsync(x => x.Id == SettingsId, cancellationToken);

        if (settings is null)
        {
            settings = new PaymentSettings { Id = SettingsId };
            _db.PaymentSettings.Add(settings);
        }

        if (qrImageData is not null)
        {
            settings.QrImageData = qrImageData;
            settings.QrImageContentType = qrImageContentType;
        }

        settings.AbaPaymentEnabled = abaPaymentEnabled;
        settings.AbaUrl = string.IsNullOrEmpty(abaUrl) ? null : abaUrl;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            abaPaymentEnabled = settings.AbaPaymentEnabled,
            abaUrl = settings.AbaUrl,
            qrImageUrl = GetQrImageUrl(
                settings.QrImageData is { Length: > 0 },
                settings.UpdatedAtUtc),
            hasUploadedQr = settings.QrImageData is { Length: > 0 },
        });
    }

    private static string GetQrImageUrl(bool hasQrImage, DateTime? updatedAtUtc)
    {
        return hasQrImage && updatedAtUtc.HasValue
            ? $"/api/payment-settings/qr?v={updatedAtUtc.Value.Ticks}"
            : DefaultQrImageUrl;
    }

    private static string? DetectImageContentType(ReadOnlySpan<byte> imageData)
    {
        if (imageData.Length >= 8 &&
            imageData[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            return "image/png";
        }

        if (imageData.Length >= 3 &&
            imageData[0] == 0xff &&
            imageData[1] == 0xd8 &&
            imageData[2] == 0xff)
        {
            return "image/jpeg";
        }

        if (imageData.Length >= 12 &&
            imageData[..4].SequenceEqual("RIFF"u8) &&
            imageData[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
}
