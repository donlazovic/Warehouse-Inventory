using System.Text.RegularExpressions;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Common;

public static partial class Guard
{
    private const decimal MaxQuantity = 1_000_000_000m;
    private const decimal MaxMoney = 10_000_000_000m;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"^[A-Z0-9][A-Z0-9-]*$")]
    private static partial Regex CodePattern();

    public static string Required(string? value, string label, int maxLength)
    {
        var text = value?.Trim() ?? string.Empty;

        if (text.Length == 0)
            throw new AppException($"Polje \"{label}\" je obavezno.");

        if (text.Length > maxLength)
            throw new AppException($"Polje \"{label}\" moze imati najvise {maxLength} karaktera.");

        return text;
    }

    public static string? Optional(string? value, string label, int maxLength)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        if (text.Length > maxLength)
            throw new AppException($"Polje \"{label}\" moze imati najvise {maxLength} karaktera.");

        return text;
    }

    public static string? Email(string? value, bool required = false, int maxLength = 200)
    {
        var text = required ? Required(value, "Email", maxLength) : Optional(value, "Email", maxLength);

        if (text is not null && !EmailPattern().IsMatch(text))
            throw new AppException("Email adresa nije ispravna.");

        return text;
    }
    public static string Code(string? value, string label, int maxLength)
    {
        var code = Required(value, label, maxLength).ToUpperInvariant();

        if (!CodePattern().IsMatch(code))
            throw new AppException($"Polje \"{label}\" moze sadrzati samo slova, cifre i crticu (npr. NIS-01).");

        return code;
    }

    public static void Defined<TEnum>(TEnum value, string label) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new AppException($"Vrednost polja \"{label}\" nije ispravna.");
    }

    public static void Money(decimal value, string label)
    {
        if (value < 0)
            throw new AppException($"Polje \"{label}\" ne moze biti negativno.");

        if (value >= MaxMoney)
            throw new AppException($"Polje \"{label}\" ima preveliku vrednost.");
    }
    public static void Quantity(decimal value, UnitOfMeasure unit, string productName, bool zeroAllowed = false)
    {
        if (value < 0 || (value == 0 && !zeroAllowed))
            throw new AppException($"Kolicina za \"{productName}\" mora biti veca od nule.");

        if (value > MaxQuantity)
            throw new AppException($"Kolicina za \"{productName}\" je prevelika.");

        if (decimal.Round(value, 3) != value)
            throw new AppException($"Kolicina za \"{productName}\" moze imati najvise tri decimale.");

        if (IsCountable(unit) && value != decimal.Truncate(value))
            throw new AppException($"\"{productName}\" se vodi u celim jedinicama, pa kolicina mora biti ceo broj.");
    }

    public static bool IsCountable(UnitOfMeasure unit)
        => unit is UnitOfMeasure.Piece or UnitOfMeasure.Package or UnitOfMeasure.Crate or UnitOfMeasure.Pallet;
}
