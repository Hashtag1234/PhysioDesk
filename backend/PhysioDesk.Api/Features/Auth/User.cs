namespace PhysioDesk.Api.Features.Auth;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }
    public string DisplayName { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    // Refresh token state. A single active refresh token per user is enough for now, but we can extend this to support multiple refresh tokens per user in the future if needed.
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
