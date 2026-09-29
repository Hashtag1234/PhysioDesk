namespace PhysioDesk.Api.Features.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, UserSummary User);

public record UserSummary(int Id, string Email, string DisplayName, string Role);
