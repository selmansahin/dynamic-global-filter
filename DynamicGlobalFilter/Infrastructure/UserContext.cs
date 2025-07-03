namespace DynamicGlobalFilter.Infrastructure;

/// <summary>
/// Request başına tenant bilgisini saklayacak service
/// </summary>
public class UserContext
{
    public int TenantId { get; set; }
}
