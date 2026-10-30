
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AdminProvisioningService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;

    public AdminProvisioningService(
        AppDbContext db,
        IPasswordHasher<AdminUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task CreateAdminAsync(
        string username,
        string password)
    {
        username = username.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");

        if (password.Length < 8)
            throw new ArgumentException(
                "Password must contain at least 8 characters.");

        var exists = await _db.AdminUsers
            .AnyAsync(x => x.Username == username);

        if (exists)
            throw new InvalidOperationException(
                "This admin username already exists.");

        var admin = new AdminUser
        {
            Username = username,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        admin.PasswordHash = _passwordHasher.HashPassword(
            admin,
            password);

        _db.AdminUsers.Add(admin);

        await _db.SaveChangesAsync();
    }
}
