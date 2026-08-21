namespace AutoHub.Models.Entities;

public class BrandEntity
{
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? WebsiteUrl { get; set; }
    public int? LogoMediaId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VehicleModelEntity> Models { get; set; } = new List<VehicleModelEntity>();
}
