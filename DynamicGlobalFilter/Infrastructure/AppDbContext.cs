using System.Linq;
using System.Reflection;
using DynamicGlobalFilter.Domain;
using Microsoft.EntityFrameworkCore;

namespace DynamicGlobalFilter.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options, UserContext userContext)
    : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tüm ITenantEntity implementasyonları için otomatik olarak global query filter ekle
         
        
        ApplyGlobalFiltersForTenantEntities(modelBuilder);
        
        // Seed data
        SeedData(modelBuilder);
    }
    
    /// <summary>
    /// Tüm ITenantEntity implementasyonları için global query filter ekler
    /// </summary>
    private void ApplyGlobalFiltersForTenantEntities(ModelBuilder modelBuilder)
    {
        // DbContext içindeki tüm DbSet<T> property'lerini bul
        var entityTypes = GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .Where(t => typeof(ITenantEntity).IsAssignableFrom(t))
            .ToList();

        // Bulunan her entity tipi için global query filter ekle
        foreach (var entityType in entityTypes)
        {
            // Generic ApplyGlobalFilterMethod metodunu çağır
            var method = typeof(AppDbContext).GetMethod(nameof(ApplyGlobalTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(entityType);
            genericMethod.Invoke(this, new object[] { modelBuilder });
        }

        // Ayrıca modelBuilder'dan da entity'leri kontrol et (DbSet olarak tanımlanmamış olabilir)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(ITenantEntity).IsAssignableFrom(clrType) && !entityTypes.Contains(clrType))
            {
                // Generic ApplyGlobalFilterMethod metodunu çağır
                var method = typeof(AppDbContext).GetMethod(nameof(ApplyGlobalTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method!.MakeGenericMethod(clrType);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }
        
    }
    
    /// <summary>
    /// Belirtilen entity tipi için tenant filtresi ekler
    /// </summary>
    private void ApplyGlobalTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == userContext.TenantId);
    }
    
    public override int SaveChanges()
    {
        // Tenant ID'yi otomatik olarak ekle
        ApplyTenantIdToAddedEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tenant ID'yi otomatik olarak ekle
        ApplyTenantIdToAddedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Yeni eklenen entity'lere otomatik olarak TenantId atar
    /// </summary>
    private void ApplyTenantIdToAddedEntities()
    {
        var addedEntities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .OfType<ITenantEntity>()
            .ToList();

        foreach (var entity in addedEntities)
        {
            entity.TenantId = userContext.TenantId;
        }
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
