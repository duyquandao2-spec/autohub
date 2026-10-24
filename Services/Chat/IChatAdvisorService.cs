using AutoHub.Models;

namespace AutoHub.Services.Chat;

public sealed record ChatVehicleCard(
    int Id,
    string Name,
    string Brand,
    string Slug,
    string PriceDisplay,
    decimal BasePrice,
    string ImagePath,
    string Category,
    string FuelType,
    string? Acceleration,
    string? Power);

public sealed record ChatActionItem(string Label, string Action, string? Href = null);

public sealed record ChatAdvisorRequest(string Message, string? CurrentSlug = null);

public sealed record ChatAdvisorResponse(
    string Content,
    IReadOnlyList<ChatVehicleCard> Vehicles,
    IReadOnlyList<ChatActionItem> Actions,
    string Intent);

public interface IChatAdvisorService
{
    Task<ChatAdvisorResponse> ProcessMessageAsync(string userMessage, string? currentSlug = null);
}
