using AutoHub.Models;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.Consultations;

[Authorize(Policy = Policies.AppointmentManagement)]
public class IndexModel : PageModel
{
    private readonly IConsultationService _consultations;

    public IndexModel(IConsultationService consultations) => _consultations = consultations;

    public IPagedResult<ConsultationListView> Paged { get; set; } = null!;

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Status { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Paged = await _consultations.GetPagedAsync(PageNumber, 15, Status, Search, null, null);
    }
}
