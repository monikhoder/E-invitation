using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class GuestWishRequest
{
    [Required]
    [StringLength(1000)]
    public string Wish { get; set; } = string.Empty;
}
