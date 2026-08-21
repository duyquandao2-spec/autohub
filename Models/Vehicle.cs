using System.Text.Json.Serialization;

namespace AutoHub.Models;

public class Vehicle
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string Drivetrain { get; set; } = string.Empty;
    public int Seats { get; set; }
    public string Price { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Acceleration { get; set; }
    public int? TopSpeed { get; set; }
    public int? Power { get; set; }
    public decimal? FuelEconomy { get; set; }
    public string? Range { get; set; }
    public string? Charging { get; set; }
    public string? FastCharging { get; set; }
    public string? Model3DUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsPopular { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]
    public string Brand => BrandName;

    [JsonIgnore]
    public VehicleSpecification? Specification { get; set; }

    [JsonIgnore]
    public List<VehicleColor> Colors { get; set; } = new();

    [JsonIgnore]
    public string AccelerationDisplay => Acceleration.HasValue ? $"{Acceleration:F1}s" : "-";

    [JsonIgnore]
    public string TopSpeedDisplay => TopSpeed.HasValue ? $"{TopSpeed} km/h" : "-";

    [JsonIgnore]
    public string PowerDisplay => Power.HasValue ? $"{Power} HP" : "-";

    [JsonIgnore]
    public string FuelEconomyDisplay => FuelEconomy.HasValue ? $"{FuelEconomy:F1} L/100km" : "-";

    [JsonIgnore]
    public string PriceDisplay => BasePrice > 0 ? $"Từ {BasePrice:N0}đ" : "Liên hệ";

    public decimal BasePrice { get; set; }
}
