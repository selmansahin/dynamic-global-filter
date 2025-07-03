using DynamicGlobalFilter.Domain;
using DynamicGlobalFilter.Features.Products.GetById;
using DynamicGlobalFilter.Infrastructure;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DynamicGlobalFilter.Features.Products.Create;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreateProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TenantId { get; set; }
}

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz")
            .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");
    }
}

public class CreateProductEndpoint : Endpoint<CreateProductRequest, CreateProductResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly UserContext _userContext;

    public CreateProductEndpoint(AppDbContext dbContext, UserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public override void Configure()
    {
        Post("/api/products");
        AllowAnonymous();
        Description(d => d
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(201, "application/json")
            .Produces(400));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        // Yeni ürün oluştur (otomatik olarak mevcut tenant'a ait)
        var product = new Product
        {
            Name = req.Name,
            Price = req.Price,
            TenantId = _userContext.TenantId // Middleware'den gelen tenant bilgisi
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(ct);

        var response = new CreateProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            TenantId = product.TenantId
        };

        await SendCreatedAtAsync<GetProductByIdEndpoint>(
            new { id = product.Id },
            response,
            generateAbsoluteUrl: true,
            cancellation: ct);
    }
}
