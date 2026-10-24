namespace AutoHub.Models.Entities;

public class AuditLogEntity
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Result { get; set; }
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AutoHub.Models.Identity.ApplicationUser? User { get; set; }
}
