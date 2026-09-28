using System.Globalization;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Export;

internal static class ExportFormatting
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("sr-Latn-RS");

    public static System.DateTime Local(System.DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Unspecified ? System.DateTime.SpecifyKind(value, DateTimeKind.Utc) : value;
        return utc.ToLocalTime();
    }

    public static System.DateTime? Local(System.DateTime? value) => value.HasValue ? Local(value.Value) : null;

    public static string Quantity(decimal value) => value.ToString("#,##0.###", Culture);
    public static string Money(decimal value) => value.ToString("#,##0.00", Culture) + " RSD";
    public static string Date(System.DateTime value) => Local(value).ToString("dd.MM.yyyy.", Culture);
    public static string Timestamp(System.DateTime value) => Local(value).ToString("dd.MM.yyyy. HH:mm", Culture);

    public static string Cell(object? value, ColumnKind kind) => value switch
    {
        null => "",
        decimal d when kind == ColumnKind.Money => Money(d),
        decimal d => Quantity(d),
        int i => i.ToString("#,##0", Culture),
        double x => x.ToString("#,##0.#", Culture),
        System.DateTime dt => Timestamp(dt),
        _ => value.ToString() ?? ""
    };

    public static string Unit(UnitOfMeasure unit) => unit switch
    {
        UnitOfMeasure.Piece => "kom",
        UnitOfMeasure.Kilogram => "kg",
        UnitOfMeasure.Gram => "g",
        UnitOfMeasure.Liter => "l",
        UnitOfMeasure.Milliliter => "ml",
        UnitOfMeasure.Package => "pak",
        UnitOfMeasure.Crate => "gajba",
        UnitOfMeasure.Pallet => "paleta",
        _ => ""
    };

    public static string Status(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Nacrt",
        OrderStatus.PendingApproval => "Čeka odobrenje",
        OrderStatus.Approved => "Odobren",
        OrderStatus.InProgress => "U realizaciji",
        OrderStatus.Completed => "Realizovan",
        OrderStatus.Cancelled => "Otkazan",
        _ => status.ToString()
    };

    public static string OrderKind(OrderType type) => type == OrderType.Inbound ? "Ulazni" : "Izlazni";

    public static string Movement(MovementType type) => type switch
    {
        MovementType.InitialStock => "Početno stanje",
        MovementType.Inbound => "Ulaz",
        MovementType.Outbound => "Izlaz",
        MovementType.Transfer => "Prenos",
        MovementType.Adjustment => "Korekcija",
        _ => type.ToString()
    };

    public static string Reason(IssueReason? reason) => reason switch
    {
        IssueReason.Sale => "Prodaja",
        IssueReason.WriteOff => "Otpis",
        IssueReason.Damage => "Lom",
        IssueReason.InternalUse => "Interna potrošnja",
        _ => ""
    };

    public static string Month(int year, int month) =>
        new System.DateTime(year, month, 1).ToString("MMMM yyyy.", Culture);
}
