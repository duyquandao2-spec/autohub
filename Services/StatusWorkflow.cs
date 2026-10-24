namespace AutoHub.Services;

public static class StatusWorkflow
{
    public static readonly IReadOnlyDictionary<string, string[]> TestDrive = new Dictionary<string, string[]>
    {
        ["PENDING"] = ["CONFIRMED", "CANCELLED"],
        ["CONFIRMED"] = ["COMPLETED", "CANCELLED"],
        ["COMPLETED"] = [],
        ["CANCELLED"] = [],
    };

    public static readonly IReadOnlyDictionary<string, string[]> Consultation = new Dictionary<string, string[]>
    {
        ["NEW"] = ["CONTACTED", "CANCELLED"],
        ["CONTACTED"] = ["SCHEDULED", "CLOSED", "CANCELLED"],
        ["SCHEDULED"] = ["CLOSED", "CANCELLED"],
        ["CLOSED"] = [],
        ["CANCELLED"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>
    {
        ["PENDING"] = "Chờ duyệt",
        ["CONFIRMED"] = "Đã xác nhận",
        ["COMPLETED"] = "Đã hoàn thành",
        ["CANCELLED"] = "Đã hủy",
        ["NEW"] = "Mới",
        ["CONTACTED"] = "Đã liên hệ",
        ["SCHEDULED"] = "Đã hẹn",
        ["CLOSED"] = "Đã chốt",
    };

    public static string Label(string status)
        => Labels.TryGetValue(status, out var label) ? label : status;

    public static string[] Next(string status, IReadOnlyDictionary<string, string[]> map)
        => map.TryGetValue(status, out var next) ? next : [];

    public static bool IsTerminal(string status, IReadOnlyDictionary<string, string[]> map)
        => Next(status, map).Length == 0;

    public static string? CheckMove(string current, string target, IReadOnlyDictionary<string, string[]> map)
    {
        if (current == target) return null;
        if (!Next(current, map).Contains(target))
            return $"Không thể chuyển trạng thái từ {Label(current)} sang {Label(target)} theo quy trình nghiệp vụ.";
        return null;
    }

    public static string BadgeClass(string status)
    {
        return status switch
        {
            "PENDING" or "NEW" or "CONTACTED" => "signal",
            "CONFIRMED" or "SCHEDULED" => "warn",
            "COMPLETED" or "CLOSED" => "ok",
            _ => "muted",
        };
    }
}