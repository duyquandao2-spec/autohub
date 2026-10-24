using AutoHub.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public class ExperienceModel : PageModel
{
    private readonly IVehicleService _vehicles;
    private readonly INavigationService _navigation;

    public ExperienceModel(IVehicleService vehicles, INavigationService navigation)
    {
        _vehicles = vehicles;
        _navigation = navigation;
    }

    public IReadOnlyList<Models.Vehicle> FeaturedVehicles { get; private set; } = [];
    public Models.Vehicle? HeroVehicle { get; private set; }

    public void OnGet()
    {
        FeaturedVehicles = _vehicles.GetVehicles()
            .Where(v => v.IsFeatured || v.IsPopular)
            .Take(8)
            .ToList();

        HeroVehicle = FeaturedVehicles.FirstOrDefault()
                      ?? _vehicles.GetVehicles().FirstOrDefault();
    }
}
