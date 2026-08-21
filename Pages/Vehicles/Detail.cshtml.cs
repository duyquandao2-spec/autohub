using AutoHub.Models;
using AutoHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Vehicles;

public sealed class DetailModel(IVehicleService vehicleService) : PageModel
{
    public Vehicle Vehicle { get; private set; } = default!;

    public IActionResult OnGet(string slug)
    {
        var vehicle = vehicleService.GetVehicleBySlug(slug);
        if (vehicle is null)
        {
            return NotFound();
        }

        Vehicle = vehicle;
        return Page();
    }
}
