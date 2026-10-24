using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoHub.Models;

namespace AutoHub.Services.Chat;

public sealed class ChatAdvisorService : IChatAdvisorService
{
    private readonly IVehicleService _vehicleService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ChatAdvisorService> _logger;

    public ChatAdvisorService(
        IVehicleService vehicleService,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<ChatAdvisorService> logger)
    {
        _vehicleService = vehicleService;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<ChatAdvisorResponse> ProcessMessageAsync(string userMessage, string? currentSlug = null)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return new ChatAdvisorResponse(
                "Xin chào! Tôi là trợ lý AI của AUTOHUB. Bạn cần tôi hỗ trợ tìm xe theo tầm giá, kiểu dáng hay tư vấn trả góp?",
                Array.Empty<ChatVehicleCard>(),
                DefaultActions(),
                "GREETING");
        }

        var allVehicles = _vehicleService.GetVehicles();

        // 1. Try calling Google Gemini AI API first if configured
        var geminiResponse = await TryCallGeminiAsync(userMessage, currentSlug, allVehicles);
        if (geminiResponse != null)
        {
            return geminiResponse;
        }

        // 2. Intelligent Internal Fallback (Rule-based / Budget / Category / Finance)
        return ProcessFallback(userMessage, allVehicles);
    }

    private async Task<ChatAdvisorResponse?> TryCallGeminiAsync(
        string userMessage,
        string? currentSlug,
        IReadOnlyList<Vehicle> allVehicles)
    {
        var apiKey = _config["GEMINI_API_KEY"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("YOUR_") || apiKey.Contains("placeholder"))
        {
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var sb = new StringBuilder();
            sb.AppendLine("DANH MỤC XE CÓ SẴN TẠI SHOWROOM AUTOHUB (VIỆT NAM):");
            foreach (var v in allVehicles)
            {
                sb.AppendLine($"- Tên xe: {v.Name} | Hãng: {v.BrandName} | Phân loại: {v.Category} | Động cơ: {v.FuelType} | Giá niêm yết: {v.PriceDisplay} ({v.BasePrice:N0} VNĐ) | Tăng tốc 0-100: {v.AccelerationDisplay} | Công suất: {v.PowerDisplay} | Dẫn động: {v.Drivetrain} | Ghế: {v.Seats} chỗ | Slug: {v.Slug}");
            }

            var systemInstruction = $@"Bạn là APEX AI — Chuyên viên tư vấn xe hơi cao cấp và kế hoạch tài chính tại showroom AUTOHUB (Việt Nam).
Dưới đây là danh mục xe hiện có trong kho showroom:
{sb}

YÊU CẦU TRẢ LỜI:
1. Bạn có kiến thức sâu rộng về ô tô, hãy giải đáp trực tiếp, khách quan và chuyên nghiệp mọi thắc mắc của khách hàng (so sánh xe, cảm giác lái, tư vấn theo ngân sách, mục đích công việc hay gia đình, công nghệ động cơ xăng/hybrid/điện EV...).
2. Luôn liên hệ và gợi ý các mẫu xe phù hợp đang có sẵn trong danh mục showroom ở trên.
3. Trả lời bằng tiếng Việt tự nhiên, súc tích (dưới 180 từ), định dạng Markdown đẹp (dùng **in đậm** tên xe, gạch đầu dòng • rõ ràng).
4. Nhắc khách hàng có thể đăng ký lái thử miễn phí hoặc sử dụng công cụ Dự toán trả góp ngân hàng trên website.";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = $"{systemInstruction}\n\n[Khách hàng hỏi]: {userMessage}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 800
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={apiKey}";
            var httpResp = await client.PostAsJsonAsync(url, requestPayload);

            if (!httpResp.IsSuccessStatusCode)
            {
                var errBody = await httpResp.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini API call returned status {StatusCode}: {Error}", httpResp.StatusCode, errBody);
                return null;
            }

            var jsonResult = await httpResp.Content.ReadFromJsonAsync<JsonElement>();
            if (!jsonResult.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text)) return null;

            // Detect vehicles mentioned in Gemini's response to render rich cards
            var matchedCards = new List<ChatVehicleCard>();
            foreach (var v in allVehicles)
            {
                if (text.Contains(v.Name, StringComparison.OrdinalIgnoreCase) ||
                    text.Contains(v.Model, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(v.Variant) && text.Contains(v.Variant, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!matchedCards.Any(c => c.Id == v.Id))
                        matchedCards.Add(MapToCard(v));
                }
                if (matchedCards.Count >= 3) break;
            }

            var actions = new List<ChatActionItem>();
            if (matchedCards.Count > 0)
            {
                actions.Add(new ChatActionItem($"Tính trả góp {matchedCards[0].Name}", "navigate", $"/Finance?vehicle={matchedCards[0].Slug}"));
                actions.Add(new ChatActionItem($"Đặt lịch lái thử {matchedCards[0].Name}", "navigate", $"/TestDrive?vehicle={matchedCards[0].Slug}"));
                actions.Add(new ChatActionItem("Xem xe 3D", "action", "view-3d"));
            }
            else
            {
                actions.Add(new ChatActionItem("Dự toán trả góp", "navigate", "/Finance"));
                actions.Add(new ChatActionItem("Đặt lịch lái thử", "navigate", "/TestDrive"));
                actions.Add(new ChatActionItem("Xem showroom", "navigate", "/#machines"));
            }

            return new ChatAdvisorResponse(text, matchedCards, actions, "GEMINI_AI");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Loi khi ket noi Gemini API, chuyen sang bo xu ly noi bo");
            return null;
        }
    }

    private ChatAdvisorResponse ProcessFallback(string userMessage, IReadOnlyList<Vehicle> allVehicles)
    {
        var text = userMessage.Trim().ToLower(CultureInfo.CurrentCulture);

        // 1. Check for Greeting
        if (Regex.IsMatch(text, @"^(xin\s+chào|chào|hello|hi|hey|alo|bạn\s+ơi)\b"))
        {
            return new ChatAdvisorResponse(
                "Chào bạn! Tôi là APEX AI — trợ lý showroom AUTOHUB. Tôi có thể giúp bạn tra cứu nhanh dòng xe trong kho, tìm xe theo ngân sách hoặc tính chi phí lăn bánh & trả góp.",
                Array.Empty<ChatVehicleCard>(),
                DefaultActions(),
                "GREETING");
        }

        // 2. Check for Finance / Installment Intent
        if (Regex.IsMatch(text, @"(trả\s*góp|vay|lãi\s*suất|lăn\s*bánh|chi\s*phí|dự\s*toán|ngân\s*hàng|bảo\s*hiểm)"))
        {
            var matchedVehicle = FindMentionedVehicle(text, allVehicles);
            if (matchedVehicle != null)
            {
                var card = MapToCard(matchedVehicle);
                return new ChatAdvisorResponse(
                    $"AUTOHUB hỗ trợ gói vay ngân hàng lên tới 80% giá trị xe **{matchedVehicle.Name}** với lãi suất ưu đãi từ 8.5%/năm. Bạn có thể sử dụng công cụ Dự toán lăn bánh & Trả góp tự động để xem chi tiết từng kỳ trả nợ.",
                    new[] { card },
                    new List<ChatActionItem>
                    {
                        new("Tính trả góp xe này", "navigate", $"/Finance?vehicle={matchedVehicle.Slug}"),
                        new("Đặt lịch lái thử", "navigate", $"/TestDrive?vehicle={matchedVehicle.Slug}"),
                        new("Xem thêm xe khác", "action", "search-vehicle")
                    },
                    "FINANCE_CALC");
            }

            return new ChatAdvisorResponse(
                "AUTOHUB có công cụ **Dự toán chi phí lăn bánh & Trả góp ngân hàng** giúp bạn tính toán chính xác số tiền trả trước, tiền lãi từng tháng và chi phí hoàn thiện biển số theo từng tỉnh thành.",
                Array.Empty<ChatVehicleCard>(),
                new List<ChatActionItem>
                {
                    new("Mở công cụ dự toán", "navigate", "/Finance"),
                    new("Tư vấn viên hỗ trợ", "consultation"),
                    new("Tìm xe phù hợp ngân sách", "action", "search-vehicle")
                },
                "FINANCE_CALC");
        }

        // 3. Check for Test Drive Intent
        if (Regex.IsMatch(text, @"(lái\s*thử|đặt\s*lịch|chạy\s*thử|trải\s*nghiệm\s*lái|hẹn\s*lái)"))
        {
            var matchedVehicle = FindMentionedVehicle(text, allVehicles);
            if (matchedVehicle != null)
            {
                return new ChatAdvisorResponse(
                    $"Tuyệt vời! Bạn có thể đặt lịch lái thử trực tiếp mẫu xe **{matchedVehicle.Name}** tại showroom AUTOHUB. Hãy chọn khung giờ thuận tiện nhất cho bạn.",
                    new[] { MapToCard(matchedVehicle) },
                    new List<ChatActionItem>
                    {
                        new("Đặt lịch lái thử xe này", "navigate", $"/TestDrive?vehicle={matchedVehicle.Slug}"),
                        new("Dự toán lăn bánh", "navigate", $"/Finance?vehicle={matchedVehicle.Slug}"),
                        new("Xem xe 3D", "action", "view-3d")
                    },
                    "TEST_DRIVE");
            }

            return new ChatAdvisorResponse(
                "Bạn có thể đăng ký lái thử bất kỳ mẫu xe nào có sẵn tại showroom AUTOHUB hoàn toàn miễn phí. Chúng tôi sẽ chuẩn bị xe và lộ trình riêng cho bạn.",
                Array.Empty<ChatVehicleCard>(),
                new List<ChatActionItem>
                {
                    new("Đăng ký lái thử ngay", "navigate", "/TestDrive"),
                    new("Xem danh sách xe", "navigate", "/#machines"),
                    new("Tư vấn nhanh", "consultation")
                },
                "TEST_DRIVE");
        }

        // 4. Check for 3D Experience Intent
        if (Regex.IsMatch(text, @"(3d|xoay\s*360|trải\s*nghiệm\s*3d|mô\s*hình\s*3d|nội\s*thất\s*3d|xem\s*xe\s*3d)"))
        {
            return new ChatAdvisorResponse(
                "Showroom số AUTOHUB trang bị công nghệ **Trải nghiệm xe 3D tương tác**. Bạn có thể xoay 360°, đổi màu sơn, bật đèn pha, mở cửa xe và lắng nghe âm thanh động cơ thực tế.",
                Array.Empty<ChatVehicleCard>(),
                new List<ChatActionItem>
                {
                    new("Mở trải nghiệm 3D ngay", "action", "view-3d"),
                    new("Khám phá công nghệ", "navigate", "/Experience"),
                    new("Về showroom xe", "navigate", "/#machines")
                },
                "EXPERIENCE_3D");
        }

        // 5. Check for Specific Model/Vehicle Query
        var directVehicle = FindMentionedVehicle(text, allVehicles);
        if (directVehicle != null)
        {
            var card = MapToCard(directVehicle);
            var desc = directVehicle.Description.Length > 160 ? directVehicle.Description[..157] + "..." : directVehicle.Description;
            var reply = $"Thông tin mẫu xe **{directVehicle.Name}**:\n" +
                        $"• Giá niêm yết: **{directVehicle.PriceDisplay}**\n" +
                        $"• Động cơ: {directVehicle.FuelType} · Dẫn động {directVehicle.Drivetrain}\n" +
                        $"• Công suất: {directVehicle.PowerDisplay} · Tăng tốc 0-100: {directVehicle.AccelerationDisplay}\n" +
                        $"• Mô tả: {desc}";

            return new ChatAdvisorResponse(
                reply,
                new[] { card },
                new List<ChatActionItem>
                {
                    new("Xem chi tiết thông số", "navigate", $"/Vehicles/{directVehicle.Slug}"),
                    new("Dự toán lăn bánh & Trả góp", "navigate", $"/Finance?vehicle={directVehicle.Slug}"),
                    new("Đặt lịch lái thử", "navigate", $"/TestDrive?vehicle={directVehicle.Slug}")
                },
                "VEHICLE_DETAIL");
        }

        // 6. Check for Budget Filtering (ví dụ: "dưới 2 tỷ", "khoảng 3 tỷ", "tầm 1.5 tỷ", "trên 4 tỷ")
        var (minPrice, maxPrice) = ExtractBudgetRange(text);
        var filteredVehicles = allVehicles.AsEnumerable();

        if (minPrice.HasValue || maxPrice.HasValue)
        {
            if (minPrice.HasValue) filteredVehicles = filteredVehicles.Where(v => v.BasePrice >= minPrice.Value);
            if (maxPrice.HasValue) filteredVehicles = filteredVehicles.Where(v => v.BasePrice <= maxPrice.Value);

            filteredVehicles = ApplyKeywordsFilter(filteredVehicles, text);

            var matches = filteredVehicles.Take(4).ToList();
            if (matches.Count > 0)
            {
                var budgetLabel = DescribeBudget(minPrice, maxPrice);
                var cards = matches.Select(MapToCard).ToList();
                return new ChatAdvisorResponse(
                    $"Dưới đây là các mẫu xe phù hợp với tầm giá **{budgetLabel}** trong kho AUTOHUB:",
                    cards,
                    new List<ChatActionItem>
                    {
                        new("Tính trả góp mẫu đầu tiên", "navigate", $"/Finance?vehicle={matches[0].Slug}"),
                        new("Lọc xe điện (EV)", "action", "filter-ev"),
                        new("Tư vấn gói vay", "consultation")
                    },
                    "BUDGET_ADVICE");
            }
            else
            {
                return new ChatAdvisorResponse(
                    $"Hiện không có mẫu xe nào sẵn hàng trong khoảng giá này. Bạn có thể tham khảo các phân khúc lân cận hoặc liên hệ chuyên viên để nhận danh sách xe chuẩn bị về showroom.",
                    Array.Empty<ChatVehicleCard>(),
                    new List<ChatActionItem>
                    {
                        new("Xem tất cả xe", "navigate", "/#machines"),
                        new("Tư vấn nhanh", "consultation")
                    },
                    "BUDGET_ADVICE");
            }
        }

        // 7. Check for Category (SUV, Sedan, EV, Sport, 7 chỗ, etc.)
        if (Regex.IsMatch(text, @"(suv|sedan|crossover|coupe|hatchback|mpv|xe\s*điện|ev|hybrid|diesel|7\s*chỗ|thể\s*thao|nhanh\s*nhất|mạnh\s*nhất)"))
        {
            var matches = ApplyKeywordsFilter(allVehicles, text).Take(4).ToList();
            if (matches.Count > 0)
            {
                var cards = matches.Select(MapToCard).ToList();
                return new ChatAdvisorResponse(
                    $"Tôi tìm thấy {matches.Count} mẫu xe nổi bật đáp ứng đúng tiêu chí bạn đang quan tâm:",
                    cards,
                    new List<ChatActionItem>
                    {
                        new("Dự toán chi phí lăn bánh", "navigate", $"/Finance?vehicle={matches[0].Slug}"),
                        new("Đăng ký lái thử", "navigate", $"/TestDrive?vehicle={matches[0].Slug}"),
                        new("Tất cả dòng xe", "navigate", "/#machines")
                    },
                    "CATEGORY_ADVICE");
            }
        }

        // 8. Check for Brand (BMW, Mercedes, Porsche, Audi, Lexus, VinFast, etc.)
        var brandMatch = FindBrandName(text, allVehicles);
        if (!string.IsNullOrEmpty(brandMatch))
        {
            var brandCars = allVehicles
                .Where(v => string.Equals(v.BrandName, brandMatch, StringComparison.OrdinalIgnoreCase))
                .Take(4)
                .ToList();

            if (brandCars.Count > 0)
            {
                return new ChatAdvisorResponse(
                    $"Bộ sưu tập thương hiệu **{brandMatch}** hiện đang có sẵn {brandCars.Count} mẫu xe tại AUTOHUB:",
                    brandCars.Select(MapToCard).ToList(),
                    new List<ChatActionItem>
                    {
                        new($"Xem thêm xe {brandMatch}", "navigate", $"/?brand={Uri.EscapeDataString(brandMatch)}#machines"),
                        new("Tính phí lăn bánh", "navigate", $"/Finance?vehicle={brandCars[0].Slug}"),
                        new("Đặt lịch lái thử", "navigate", $"/TestDrive?vehicle={brandCars[0].Slug}")
                    },
                    "BRAND_ADVICE");
            }
        }

        // 9. General Smart Fallback
        var featured = allVehicles.Where(v => v.IsFeatured || v.IsPopular).Take(3).Select(MapToCard).ToList();
        return new ChatAdvisorResponse(
            "Tôi có thể hỗ trợ bạn lọc xe theo nhiều tiêu chí như: ngân sách (VD: *'tầm 2 tỷ'*, *'dưới 3 tỷ'*), kiểu dáng (*'SUV'*, *'Sedan 7 chỗ'*), động cơ (*'xe điện EV'*) hoặc tính toán chi phí trả góp ngân hàng.",
            featured,
            DefaultActions(),
            "GENERAL_HELP");
    }

    private static Vehicle? FindMentionedVehicle(string text, IReadOnlyList<Vehicle> vehicles)
    {
        return vehicles.FirstOrDefault(v =>
            text.Contains(v.Name.ToLower()) ||
            text.Contains(v.Model.ToLower()) ||
            text.Contains(v.Slug.ToLower().Replace("-", " ")) ||
            (!string.IsNullOrEmpty(v.Variant) && text.Contains(v.Variant.ToLower())));
    }

    private static string? FindBrandName(string text, IReadOnlyList<Vehicle> vehicles)
    {
        var brands = vehicles.Select(v => v.BrandName).Distinct();
        foreach (var b in brands)
        {
            if (text.Contains(b.ToLower())) return b;
        }

        if (text.Contains("mercedes") || text.Contains("mẹc")) return "Mercedes-Benz";
        if (text.Contains("bimmer")) return "BMW";
        if (text.Contains("vin")) return "VinFast";
        return null;
    }

    private static (decimal? Min, decimal? Max) ExtractBudgetRange(string text)
    {
        decimal? min = null;
        decimal? max = null;

        var underMatch = Regex.Match(text, @"(dưới|<|khoảng\s*dưới|tầm\s*dưới)\s*([0-9]+([.,][0-9]+)?)\s*(tỷ|ti|tỉ|triệu|tr)");
        if (underMatch.Success)
        {
            var val = ParseNumber(underMatch.Groups[2].Value);
            var unit = underMatch.Groups[4].Value;
            max = ConvertToVnd(val, unit);
            return (min, max);
        }

        var overMatch = Regex.Match(text, @"(trên|>|từ)\s*([0-9]+([.,][0-9]+)?)\s*(tỷ|ti|tỉ|triệu|tr)\s*(trở\s*lên)?");
        if (overMatch.Success)
        {
            var val = ParseNumber(overMatch.Groups[2].Value);
            var unit = overMatch.Groups[4].Value;
            min = ConvertToVnd(val, unit);
            return (min, max);
        }

        var approxMatch = Regex.Match(text, @"(tầm|khoảng|cỡ|có)\s*([0-9]+([.,][0-9]+)?)\s*(tỷ|ti|tỉ|triệu|tr)");
        if (approxMatch.Success)
        {
            var val = ParseNumber(approxMatch.Groups[2].Value);
            var unit = approxMatch.Groups[4].Value;
            var center = ConvertToVnd(val, unit);
            min = center * 0.75m;
            max = center * 1.25m;
            return (min, max);
        }

        var directMatch = Regex.Match(text, @"\b([0-9]+([.,][0-9]+)?)\s*(tỷ|ti|tỉ)\b");
        if (directMatch.Success)
        {
            var val = ParseNumber(directMatch.Groups[1].Value);
            var center = val * 1_000_000_000m;
            min = center * 0.75m;
            max = center * 1.25m;
            return (min, max);
        }

        return (min, max);
    }

    private static decimal ParseNumber(string str)
    {
        str = str.Replace(',', '.');
        return decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static decimal ConvertToVnd(decimal val, string unit)
    {
        if (unit.StartsWith("tỷ") || unit.StartsWith("ti") || unit.StartsWith("tỉ"))
            return val * 1_000_000_000m;
        return val * 1_000_000m;
    }

    private static string DescribeBudget(decimal? min, decimal? max)
    {
        if (min.HasValue && max.HasValue) return $"{min.Value / 1_000_000_000m:0.#} — {max.Value / 1_000_000_000m:0.#} tỷ";
        if (max.HasValue) return $"dưới {max.Value / 1_000_000_000m:0.#} tỷ";
        if (min.HasValue) return $"từ {min.Value / 1_000_000_000m:0.#} tỷ trở lên";
        return "mọi mức giá";
    }

    private static IEnumerable<Vehicle> ApplyKeywordsFilter(IEnumerable<Vehicle> source, string text)
    {
        var list = source.ToList();

        if (text.Contains("suv") || text.Contains("gầm cao"))
            list = list.Where(v => v.Category.Contains("SUV", StringComparison.OrdinalIgnoreCase) || v.Category.Contains("Crossover", StringComparison.OrdinalIgnoreCase)).ToList();
        else if (text.Contains("sedan"))
            list = list.Where(v => v.Category.Contains("Sedan", StringComparison.OrdinalIgnoreCase)).ToList();
        else if (text.Contains("coupe"))
            list = list.Where(v => v.Category.Contains("Coupe", StringComparison.OrdinalIgnoreCase)).ToList();

        if (text.Contains("điện") || text.Contains("ev"))
            list = list.Where(v => v.FuelType.Equals("EV", StringComparison.OrdinalIgnoreCase) || v.FuelType.Contains("điện", StringComparison.OrdinalIgnoreCase)).ToList();
        else if (text.Contains("hybrid"))
            list = list.Where(v => v.FuelType.Contains("Hybrid", StringComparison.OrdinalIgnoreCase)).ToList();

        if (text.Contains("7 chỗ"))
            list = list.Where(v => v.Seats >= 7).ToList();

        if (text.Contains("nhanh") || text.Contains("thể thao") || text.Contains("tăng tốc"))
            list = list.OrderBy(v => v.Acceleration ?? 99).ToList();

        return list;
    }

    private static ChatVehicleCard MapToCard(Vehicle v)
    {
        return new ChatVehicleCard(
            v.Id,
            v.Name,
            v.BrandName,
            v.Slug,
            v.PriceDisplay,
            v.BasePrice,
            v.ImagePath,
            v.Category,
            v.FuelType,
            v.AccelerationDisplay,
            v.PowerDisplay);
    }

    private static IReadOnlyList<ChatActionItem> DefaultActions() => new List<ChatActionItem>
    {
        new("Tìm xe dưới 2 tỷ", "query", "Tìm xe dưới 2 tỷ"),
        new("Xe SUV nổi bật", "query", "Gợi ý các dòng xe SUV"),
        new("Xe điện EV", "action", "filter-ev"),
        new("Dự toán trả góp", "navigate", "/Finance"),
        new("Đặt lịch lái thử", "navigate", "/TestDrive")
    };
}
