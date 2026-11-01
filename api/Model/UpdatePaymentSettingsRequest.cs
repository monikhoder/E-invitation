using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace api.Models;

public class UpdatePaymentSettingsRequest
{
    [StringLength(2000)]
    public string? AbaUrl { get; set; }

    [Required]
    public bool? AbaPaymentEnabled { get; set; }

    public IFormFile? QrImage { get; set; }
}
