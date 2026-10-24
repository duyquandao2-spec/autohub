using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Account;

[Authorize]
public class ConsultationsModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly IConsultationService _consultations;

    public ConsultationsModel(ICurrentUser current, IConsultationService consultations)
    {
        _current = current;
        _consultations = consultations;
    }

    public List<ConsultationRequestEntity> Consultations { get; set; } = new();

    public async Task OnGetAsync()
    {
        Consultations = await _consultations.GetMineAsync(_current.UserId!);
    }
}
