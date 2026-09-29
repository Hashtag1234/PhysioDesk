using Microsoft.EntityFrameworkCore;
using PhysioDesk.Api.Common.Persistence;

namespace PhysioDesk.Api.Features.Auth;

public class AuthResult
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public User? User { get; init; }
    public string? AccessToken { get; init; }
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public string? RawRefreshToken { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }

    public static AuthResult Fail(string error) => new() { Succeeded = false, Error = error };
}

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email && u.IsActive, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return AuthResult.Fail("Invalid email or password.");
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResult> RefreshAsync(string rawRefreshToken, CancellationToken ct)
    {
        var hash = _tokenService.HashRefreshToken(rawRefreshToken);
        var user = await _db.Users.SingleOrDefaultAsync(
            u => u.RefreshTokenHash == hash && u.IsActive, ct);

        if (user is null || user.RefreshTokenExpiresAtUtc is null || user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            return AuthResult.Fail("Invalid or expired refresh token.");
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task LogoutAsync(int userId, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([userId], ct);
        if (user is null) return;

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAtUtc = null;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, accessExpiresAtUtc) = _tokenService.CreateAccessToken(user);
        var (rawRefresh, refreshHash, refreshExpiresAtUtc) = _tokenService.CreateRefreshToken();

        // Rotation: every issuance (login or refresh) overwrites the stored hash,
        // so a stolen-then-reused old refresh token stops working once the
        // legitimate client refreshes.
        user.RefreshTokenHash = refreshHash;
        user.RefreshTokenExpiresAtUtc = refreshExpiresAtUtc;
        await _db.SaveChangesAsync(ct);

        return new AuthResult
        {
            Succeeded = true,
            User = user,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExpiresAtUtc,
            RawRefreshToken = rawRefresh,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc
        };
    }
}
