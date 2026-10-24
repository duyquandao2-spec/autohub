namespace AutoHub.Models.Entities;

public class FavoriteEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AutoHub.Models.Identity.ApplicationUser User { get; set; } = null!;
    public VehicleVariantEntity Vehicle { get; set; } = null!;
}
