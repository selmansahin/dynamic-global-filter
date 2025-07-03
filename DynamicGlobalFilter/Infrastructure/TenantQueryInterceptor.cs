using System.Data.Common;
using DynamicGlobalFilter.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DynamicGlobalFilter.Infrastructure;

/// <summary>
/// Tüm sorguları tenant ID'ye göre filtrelemek için interceptor
/// </summary>
public class TenantQueryInterceptor : DbCommandInterceptor
{
    private readonly UserContext _userContext;

    public TenantQueryInterceptor(UserContext userContext)
    {
        _userContext = userContext;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        ManipulateCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ManipulateCommand(command);
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    private void ManipulateCommand(DbCommand command)
    {
        // Eğer TenantId parametresi zaten varsa, değerini güncelle
        var parameter = command.Parameters
            .Cast<DbParameter>()
            .FirstOrDefault(p => p.ParameterName == "TenantId");

        if (parameter != null)
        {
            parameter.Value = _userContext.TenantId;
            return;
        }

        // Eğer sorgu Products tablosuna erişiyorsa ve WHERE içermiyorsa, tenant filtresi ekle
        if (command.CommandText.Contains("FROM \"Products\"") || command.CommandText.Contains("FROM [Products]"))
        {
            // Eğer WHERE yoksa ekle, varsa AND ile ekle
            if (!command.CommandText.Contains("WHERE"))
            {
                command.CommandText = command.CommandText + " WHERE \"TenantId\" = @TenantId";
            }
            else if (!command.CommandText.Contains("\"TenantId\" ="))
            {
                // WHERE kelimesinden sonra tenant filtresi ekle
                var whereIndex = command.CommandText.IndexOf("WHERE") + 5;
                command.CommandText = command.CommandText.Insert(whereIndex, " \"TenantId\" = @TenantId AND");
            }

            // TenantId parametresi ekle
            var tenantIdParam = command.CreateParameter();
            tenantIdParam.ParameterName = "TenantId";
            tenantIdParam.Value = _userContext.TenantId;
            command.Parameters.Add(tenantIdParam);
        }
    }
}
