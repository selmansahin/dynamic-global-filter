namespace DynamicGlobalFilter.Domain;

public class Product : ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TenantId { get; set; } // Multi-tenant için
}
