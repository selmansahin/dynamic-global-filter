using DynamicGlobalFilter.Infrastructure;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace DynamicGlobalFilter.Features.Products.List;

public class ListProductsRequest
{
    // FastEndpoints en az bir public property gerektiriyor
    public int Page { get; set; } = 1;
}

public class ListProductsResponse
{
    public List<ProductDto> Products { get; set; } = new();
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TenantId { get; set; }
}

public class ListProductsEndpoint : Endpoint<ListProductsRequest, ListProductsResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly UserContext _userContext;

    public ListProductsEndpoint(AppDbContext dbContext, UserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public override void Configure()
    {
        Get("/api/products");
        AllowAnonymous();
        Description(d => d
            .WithName("GetAllProducts")
            .Produces<ListProductsResponse>(200, "application/json")
            .Produces(400));
    }

    public override async Task HandleAsync(ListProductsRequest req, CancellationToken ct)
    {
        // Global filter sayesinde otomatik olarak sadece mevcut tenant'a ait ürünler gelecek
        var products = await _dbContext.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                TenantId = p.TenantId
            })
            .ToListAsync(ct);

        await SendAsync(new ListProductsResponse
        {
            Products = products
        }, cancellation: ct);
    }
}
