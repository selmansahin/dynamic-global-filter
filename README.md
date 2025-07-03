# .NET Core Multi-Tenant POC Projesi

Bu proje, Entity Framework Core'un Global Query Filter özelliğini kullanarak çok kiracılı (multi-tenant) bir mimarinin nasıl uygulanabileceğini gösteren bir Proof of Concept (POC) çalışmasıdır.

## Teknolojiler

- .NET Core 8
- FastEndpoints
- Entity Framework Core (PostgreSQL)
- Vertical Slice Architecture

## Proje Özellikleri

### Multi-Tenant Yapı

- `ITenantEntity` arayüzü ile tenant-aware entity'ler işaretlenir
- `UserContext` sınıfı ile her request için tenant bilgisi saklanır
- `TenantMiddleware` ile request header'dan X-Tenant-Id alınıp UserContext'e aktarılır
- Reflection kullanılarak tüm `ITenantEntity` implementasyonları için otomatik global query filter eklenir
- Yeni entity'ler eklenirken sadece `ITenantEntity` arayüzünü uygulamaları yeterlidir, başka bir kod değişikliği gerekmez
- `SaveChanges` ve `SaveChangesAsync` override edilerek yeni eklenen entity'lere otomatik TenantId ataması yapılır

### API Endpoints

- `GET /api/products` - Tenant'a göre filtrelenmiş ürünleri listeler
- `POST /api/products` - Yeni ürün ekler (otomatik olarak mevcut tenant'a)
- `GET /api/products/{id}` - Belirli bir ürünü getirir (tenant kontrolü ile)

## Proje Yapısı

```
DynamicGlobalFilter/
├── Domain/                  # Domain nesneleri
│   └── Product.cs           # Ürün entity'si
├── Features/                # Vertical slice mimarisi
│   └── Products/            # Ürün özellikleri
│       ├── Create/          # Ürün oluşturma endpoint'i
│       ├── GetById/         # ID'ye göre ürün getirme endpoint'i
│       └── List/            # Ürün listeleme endpoint'i
├── Infrastructure/          # Altyapı bileşenleri
│   ├── AppDbContext.cs      # EF Core DbContext
│   ├── TenantMiddleware.cs  # X-Tenant-Id header işleme
│   └── UserContext.cs       # Tenant bilgisi saklama
└── Program.cs               # Uygulama başlangıç noktası
```

## Kurulum

1. PostgreSQL veritabanı bağlantı bilgilerini `appsettings.json` dosyasında güncelleyin:

```json
"ConnectionStrings": {
  "PostgreSQL": "Host=localhost;Port=5432;Database=concurrency_db;Username=postgres;Password=yourpassword"
}
```

2. Projeyi derleyin ve çalıştırın:

```bash
dotnet build
dotnet run
```

## Test

Swagger UI: http://localhost:5230/swagger

Postman veya curl ile test edebilirsiniz:

### Ürünleri Listeleme (Tenant 1)

```bash
curl --location 'http://localhost:5230/api/products' \
--header 'X-Tenant-Id: 1'
```

### Ürünleri Listeleme (Tenant 2)

```bash
curl --location 'http://localhost:5230/api/products' \
--header 'X-Tenant-Id: 2'
```

### Yeni Ürün Ekleme

```bash
curl --location 'http://localhost:5230/api/products' \
--header 'Content-Type: application/json' \
--header 'X-Tenant-Id: 1' \
--data '{
    "name": "Yeni Ürün",
    "price": 500
}'
```

### ID'ye Göre Ürün Getirme

```bash
curl --location 'http://localhost:5230/api/products/1' \
--header 'X-Tenant-Id: 1'
```

## Önemli Notlar

- Bu proje bir POC çalışmasıdır ve production kullanımı için ek güvenlik önlemleri gerektirir
- Authentication ve authorization özellikleri eklenmemiştir
- X-Tenant-Id header'ı olmadan yapılan istekler reddedilir
- Global query filter sayesinde, bir tenant'ın diğer tenant'ların verilerine erişmesi engellenir

## Lisans

MIT
