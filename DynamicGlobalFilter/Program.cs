using DynamicGlobalFilter.Infrastructure;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// FastEndpoints
builder.Services.AddFastEndpoints();

// Entity Framework Core ve PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// UserContext servisini scoped olarak ekle
builder.Services.AddScoped<UserContext>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multi-Tenant API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi-Tenant API v1"));
}

// TenantMiddleware'i ekle
app.UseMiddleware<TenantMiddleware>();

// FastEndpoints middleware
app.UseFastEndpoints();

// Veritabanını oluştur ve migrate et
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
