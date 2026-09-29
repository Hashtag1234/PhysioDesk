namespace PhysioDesk.Api.Features.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);

    /// <summary>Returns the raw refresh token (given to the client) and its hash (stored on the user).</summary>
    (string RawToken, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken();

    string HashRefreshToken(string rawToken);
}
