using System.Net;

namespace DynamicGlobalFilter.Infrastructure;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        // Request header'ından "X-Tenant-Id" bilgisini oku
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header zorunludur" });
            return;
        }

        // TenantId'yi parse et
        if (!int.TryParse(tenantIdHeader, out var tenantId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id geçerli bir sayı olmalıdır" });
            return;
        }

        // UserContext'e TenantId'yi set et
        userContext.TenantId = tenantId;

        // Pipeline'a devam et
        await _next(context);
    }
}
