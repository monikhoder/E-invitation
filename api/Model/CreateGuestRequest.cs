using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class CreateGuestRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Phone { get; set; }
}
