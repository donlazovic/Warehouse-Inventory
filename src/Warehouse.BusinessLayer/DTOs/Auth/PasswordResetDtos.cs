namespace Warehouse.BusinessLayer.DTOs.Auth;

public record ForgotPasswordRequest(string Email);

public record ResetPasswordWithTokenRequest(string Token, string NewPassword);

public record ResetTokenStatusDto(bool IsValid, string? Email);
