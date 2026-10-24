using AutoHub.Data;
using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace AutoHub.Services.Auth;

public interface IAuditService
{
    Task RecordAsync(string action, string? entityType, string? entityId, string? result = null, string? metadata = null, string? userId = null);
}

public sealed class AuditService : IAuditService
{
    private readonly AutoHubDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IHttpContextAccessor _http;

    public AuditService(AutoHubDbContext db, ICurrentUser current, IHttpContextAccessor http)
    {
        _db = db;
        _current = current;
        _http = http;
    }

    public async Task RecordAsync(string action, string? entityType, string? entityId, string? result = null, string? metadata = null, string? userId = null)
    {
        var identity = _current;
        var ip = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        _db.AuditLogs.Add(new AuditLogEntity
        {
            UserId = userId ?? identity.UserId,
            UserName = userId ?? identity.UserName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Result = result ?? "SUCCESS",
            IpAddress = ip,
            Metadata = metadata,
        });

        try { await _db.SaveChangesAsync(); } catch { /* audit failures must never break a mutation */ }
    }
}
