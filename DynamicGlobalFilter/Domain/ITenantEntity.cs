namespace DynamicGlobalFilter.Domain;

/// <summary>
/// Multi-tenant entity'leri için arayüz
/// </summary>
public interface ITenantEntity
{
    int TenantId { get; set; }
}
