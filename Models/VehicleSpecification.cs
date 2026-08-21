namespace AutoHub.Models;

public class VehicleSpecification
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string? Engine { get; set; }
    public int? Displacement { get; set; }
    public int? CylinderCount { get; set; }
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public decimal? Acceleration { get; set; }
    public int? TopSpeed { get; set; }
    public decimal? FuelConsumptionCity { get; set; }
    public decimal? FuelConsumptionHighway { get; set; }
    public decimal? FuelConsumptionCombined { get; set; }
    public int? FuelTank { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Wheelbase { get; set; }
    public int? GroundClearance { get; set; }
}
