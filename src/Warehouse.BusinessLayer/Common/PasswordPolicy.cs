using System.Text;

namespace Warehouse.BusinessLayer.Common;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxBytes = 72;

    public static void Validate(string? password, string label = "Lozinka")
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
            throw new AppException($"{label} mora imati najmanje {MinLength} karaktera.");

        if (Encoding.UTF8.GetByteCount(password) > MaxBytes)
            throw new AppException(
                $"{label} je predugacka. Dozvoljeno je najvise {MaxBytes} karaktera, a slova sa kvacicama racunaju se kao dva.");
    }
}
