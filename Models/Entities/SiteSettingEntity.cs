namespace AutoHub.Models.Entities;

public class SiteSettingEntity
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Group { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
