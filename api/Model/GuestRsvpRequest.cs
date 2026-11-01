using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class GuestRsvpRequest
{
    [Required]
    [Range(0, 5)]
    public int? GuestCount { get; set; }
}
