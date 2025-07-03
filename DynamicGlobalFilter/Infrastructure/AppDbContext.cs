using DynamicGlobalFilter.Domain;
using Microsoft.EntityFrameworkCore;

namespace DynamicGlobalFilter.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly UserContext _userContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, UserContext userContext)
        : base(options)
    {
        _userContext = userContext;
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter - Tüm sorgular otomatik olarak TenantId'ye göre filtrelensin
        modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == _userContext.TenantId);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Tenant 1 için örnek ürünler
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Tenant 1 - Ürün 1", Price = 100, TenantId = 1 },
            new Product { Id = 2, Name = "Tenant 1 - Ürün 2", Price = 200, TenantId = 1 },
            new Product { Id = 3, Name = "Tenant 1 - Ürün 3", Price = 300, TenantId = 1 }
        );

        // Tenant 2 için örnek ürünler
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 4, Name = "Tenant 2 - Ürün 1", Price = 150, TenantId = 2 },
            new Product { Id = 5, Name = "Tenant 2 - Ürün 2", Price = 250, TenantId = 2 }
        );

        // Tenant 3 için örnek ürünler
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 6, Name = "Tenant 3 - Ürün 1", Price = 175, TenantId = 3 },
            new Product { Id = 7, Name = "Tenant 3 - Ürün 2", Price = 275, TenantId = 3 },
            new Product { Id = 8, Name = "Tenant 3 - Ürün 3", Price = 375, TenantId = 3 }
        );
    }
}
