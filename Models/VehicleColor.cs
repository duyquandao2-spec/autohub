namespace AutoHub.Models;

public class VehicleColor
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string? ExtraPrice { get; set; }
}
