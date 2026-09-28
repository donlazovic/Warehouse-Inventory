namespace Warehouse.BusinessLayer.Settings;

public class CompanySettings
{
    public const string SectionName = "Company";

    public string Name { get; set; } = "Skladišnik d.o.o.";
    public string Address { get; set; } = "Bulevar Nemanjića 1";
    public string City { get; set; } = "18000 Niš";
    public string TaxNumber { get; set; } = "100000001";
    public string RegistrationNumber { get; set; } = "20000001";
    public string Phone { get; set; } = "018 000 000";
}
