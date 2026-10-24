using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin;

[Authorize(Policy = Policies.SystemConfiguration)]
public class SettingsModel : PageModel
{
    private readonly ISettingsService _settings;

    public SettingsModel(ISettingsService settings) => _settings = settings;

    [BindProperty]
    public string StorefrontName { get; set; } = string.Empty;

    [BindProperty]
    public string ContactEmail { get; set; } = string.Empty;

    [BindProperty]
    public string ContactPhone { get; set; } = string.Empty;

    [BindProperty]
    public string Hotline { get; set; } = string.Empty;

    [BindProperty]
    public string Address { get; set; } = string.Empty;

    public IReadOnlyList<ShowroomEntity> Showrooms { get; set; } = Array.Empty<ShowroomEntity>();

    // Showroom create fields
    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public string? NewCode { get; set; }

    [BindProperty]
    public string? NewAddress { get; set; }

    [BindProperty]
    public string? NewPhone { get; set; }

    [BindProperty]
    public string? NewEmail { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        await _settings.SetAsync("storefront_name", StorefrontName, "store");
        await _settings.SetAsync("contact_email", ContactEmail, "store");
        await _settings.SetAsync("contact_phone", ContactPhone, "store");
        await _settings.SetAsync("hotline", Hotline, "store");
        await _settings.SetAsync("address", Address, "store");
        StatusMessage = "Đã lưu cấu hình.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostShowroomAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewName))
        {
            await _settings.SaveShowroomAsync(new ShowroomEntity
            {
                Name = NewName.Trim(),
                Code = NewCode,
                Address = NewAddress,
                Phone = NewPhone,
                Email = NewEmail,
                IsActive = true,
            });
            StatusMessage = "Đã thêm showroom.";
        }
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var all = await _settings.GetAllAsync("store");
        StorefrontName = all.GetValueOrDefault("storefront_name", "AUTOHUB");
        ContactEmail = all.GetValueOrDefault("contact_email", "");
        ContactPhone = all.GetValueOrDefault("contact_phone", "");
        Hotline = all.GetValueOrDefault("hotline", "");
        Address = all.GetValueOrDefault("address", "");
        Showrooms = await _settings.GetShowroomsAsync();
    }
}