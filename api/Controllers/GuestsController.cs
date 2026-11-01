using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/guests")]
public class GuestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public GuestsController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet("wishes")]
    public async Task<IActionResult> ListWishes(CancellationToken cancellationToken)
    {
        var wishes = await _db.Guests
            .AsNoTracking()
            .Where(x => x.Wish != null && x.Wish != "")
            .OrderByDescending(x => x.Id)
            .Take(50)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Wish
            })
            .ToListAsync(cancellationToken);

        return Ok(wishes);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var guests = await _db.Guests
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var guest in guests.Where(x =>
                     x.InvitationToken is { Length: > 16 }))
        {
            guest.InvitationToken = await CreateUniqueInvitationTokenAsync(cancellationToken);
        }

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        var baseUrl = _configuration["Invitation:BaseUrl"];
        if (guests.Any(x => x.InvitationToken is not null) &&
            (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var invitationBaseUri) ||
             (invitationBaseUri.Scheme != Uri.UriSchemeHttp &&
              invitationBaseUri.Scheme != Uri.UriSchemeHttps)))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Invitation base URL is not configured.",
                detail: "Set Invitation:BaseUrl to the public web application URL.");
        }

        var result = guests.Select(guest => new
        {
            guest.Id,
            guest.Name,
            guest.Phone,
            guest.IsConfirmed,
            guest.RsvpGuestCount,
            guest.Wish,
            InvitationUrl = guest.InvitationToken is null
                ? null
                : new Uri(
                    new Uri(baseUrl!, UriKind.Absolute),
                    $"invite/{Uri.EscapeDataString(guest.InvitationToken)}").ToString()
        });

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["Invitation:BaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var invitationBaseUri) ||
            (invitationBaseUri.Scheme != Uri.UriSchemeHttp &&
             invitationBaseUri.Scheme != Uri.UriSchemeHttps))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Invitation base URL is not configured.",
                detail: "Set Invitation:BaseUrl to the public web application URL.");
        }

        var invitationToken = await CreateUniqueInvitationTokenAsync(cancellationToken);
        var guest = new Guest
        {
            Name = request.Name.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            InvitationToken = invitationToken
        };

        _db.Guests.Add(guest);
        await _db.SaveChangesAsync(cancellationToken);

        var invitationUrl = new Uri(
            invitationBaseUri,
            $"invite/{Uri.EscapeDataString(invitationToken)}").ToString();

        return CreatedAtAction(
            nameof(GetByInvitationToken),
            new { token = invitationToken },
            new
            {
                guest.Id,
                guest.Name,
                invitationUrl
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var guest = await _db.Guests.FindAsync([id], cancellationToken);
        if (guest is null)
        {
            return NotFound();
        }

        _db.Guests.Remove(guest);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{token}")]
    public async Task<IActionResult> GetByInvitationToken(
        string token,
        CancellationToken cancellationToken)
    {
        var guest = await _db.Guests
            .AsNoTracking()
            .Where(x => x.InvitationToken == token)
            .Select(x => new
            {
                x.Name,
                x.IsConfirmed,
                x.Wish,
                x.RsvpGuestCount
            })
            .SingleOrDefaultAsync(cancellationToken);

        return guest is null ? NotFound() : Ok(guest);
    }

    [AllowAnonymous]
    [HttpPost("{token}/rsvp")]
    public async Task<IActionResult> SaveRsvp(
        string token,
        [FromBody] GuestRsvpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.GuestCount is not int guestCount)
        {
            return BadRequest(new { message = "Guest count is required." });
        }

        var guest = await _db.Guests
            .SingleOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);

        if (guest is null)
        {
            return NotFound();
        }

        guest.RsvpGuestCount = guestCount;
        guest.IsConfirmed = guestCount > 0;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            guestCount = guest.RsvpGuestCount,
            guest.IsConfirmed
        });
    }

    [AllowAnonymous]
    [HttpPost("{token}/wish")]
    public async Task<IActionResult> SaveWish(
        string token,
        [FromBody] GuestWishRequest request,
        CancellationToken cancellationToken)
    {
        var wish = request.Wish.Trim();
        if (string.IsNullOrWhiteSpace(wish))
        {
            return BadRequest(new { message = "Wish must not be empty." });
        }

        var guest = await _db.Guests
            .SingleOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);

        if (guest is null)
        {
            return NotFound();
        }

        guest.Wish = wish;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { wish = guest.Wish });
    }

    private async Task<string> CreateUniqueInvitationTokenAsync(
        CancellationToken cancellationToken)
    {
        string token;
        do
        {
            token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
        while (await _db.Guests.AnyAsync(
                   x => x.InvitationToken == token,
                   cancellationToken));

        return token;
    }
}
