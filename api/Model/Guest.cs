namespace api.Models
{
    public class Guest
    {

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsConfirmed { get; set; }

    public int? RsvpGuestCount { get; set; }

    public string? InvitationToken { get; set; }

    public string? Wish { get; set; }
    }
}