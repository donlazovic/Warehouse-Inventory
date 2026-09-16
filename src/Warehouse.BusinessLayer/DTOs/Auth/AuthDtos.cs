namespace Warehouse.BusinessLayer.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    CurrentUserDto User);

public record CurrentUserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    IReadOnlyList<string> Permissions);
