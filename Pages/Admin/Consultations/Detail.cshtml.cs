using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Models.Identity;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Admin.Consultations;

[Authorize(Policy = Policies.AppointmentManagement)]
public class DetailModel : PageModel
{
    private readonly IConsultationService _consultations;
    private readonly ICurrentUser _current;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AutoHubDbContext _db;

    public DetailModel(IConsultationService consultations, ICurrentUser current,
        UserManager<ApplicationUser> users, AutoHubDbContext db)
    {
        _consultations = consultations;
        _current = current;
        _users = users;
        _db = db;
    }

    public ConsultationRequestEntity? Consultation { get; set; }
    public List<ApplicationUser> Staff { get; set; } = new();
    public IReadOnlyList<string> StatusOptions { get; set; } = Array.Empty<string>();
    public bool IsTerminal { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Consultation = await _consultations.GetAsync(id);
        if (Consultation is null) return NotFound();
        StatusOptions = BuildStatusOptions(Consultation.Status);
        IsTerminal = StatusWorkflow.IsTerminal(Consultation.Status, StatusWorkflow.Consultation);
        await LoadStaffAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id, string status, string? assignedStaffId)
    {
        var result = await _consultations.UpdateAsync(id, status, assignedStaffId);
        if (!result.Success)
        {
            TempData["Err"] = result.Error;
            return RedirectToPage(new { id });
        }
        TempData["Ok"] = "Đã cập nhật trạng thái.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostNoteAsync(int id, string note)
    {
        var name = User.Identity?.Name ?? "staff";
        await _consultations.AddStaffNoteAsync(id, _current.UserId ?? "system", name, note);
        return RedirectToPage(new { id });
    }

    private static IReadOnlyList<string> BuildStatusOptions(string current)
    {
        var set = new List<string> { current };
        set.AddRange(StatusWorkflow.Next(current, StatusWorkflow.Consultation));
        return [.. set.Distinct()];
    }

    private async Task LoadStaffAsync()
    {
        var set = new HashSet<string>();
        foreach (var role in RoleNames.AppointmentManagers)
        {
            foreach (var u in await _users.GetUsersInRoleAsync(role))
                set.Add(u.Id);
        }
        Staff = await _db.Users.AsNoTracking()
            .Where(u => set.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }
}
