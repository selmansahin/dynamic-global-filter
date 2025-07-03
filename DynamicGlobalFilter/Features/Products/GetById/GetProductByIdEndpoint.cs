using DynamicGlobalFilter.Infrastructure;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace DynamicGlobalFilter.Features.Products.GetById;

public class GetProductByIdRequest
{
    public int Id { get; set; }
}

public class GetProductByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TenantId { get; set; }
}

public class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, GetProductByIdResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly UserContext _userContext;

    public GetProductByIdEndpoint(AppDbContext dbContext, UserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public override void Configure()
    {
        Get("/api/products/{id}");
        AllowAnonymous();
        Description(d => d
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(200, "application/json")
            .Produces(404));
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        // Global filter sayesinde otomatik olarak sadece mevcut tenant'a ait ürünler aranacak
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        if (product == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var response = new GetProductByIdResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            TenantId = product.TenantId
        };

        await SendAsync(response, cancellation: ct);
    }
}
