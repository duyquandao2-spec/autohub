using System.ComponentModel.DataAnnotations;
using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public sealed class FinanceModel : PageModel
{
    private readonly IVehicleService _vehicleService;
    private readonly IConsultationService _consultationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<FinanceModel> _logger;

    public FinanceModel(
        IVehicleService vehicleService,
        IConsultationService consultationService,
        ICurrentUser currentUser,
        ILogger<FinanceModel> logger)
    {
        _vehicleService = vehicleService;
        _consultationService = consultationService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public IReadOnlyList<Vehicle> Vehicles { get; private set; } = Array.Empty<Vehicle>();
    public Vehicle? SelectedVehicle { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Vehicle { get; set; }

    [BindProperty]
    public FinanceQuoteInput Input { get; set; } = new();

    public bool Submitted { get; private set; }
    public string ConsultationCode { get; private set; } = string.Empty;

    public void OnGet()
    {
        Vehicles = _vehicleService.GetVehicles();

        if (!string.IsNullOrWhiteSpace(Vehicle))
        {
            SelectedVehicle = Vehicles.FirstOrDefault(v =>
                string.Equals(v.Slug, Vehicle, StringComparison.OrdinalIgnoreCase));
        }

        SelectedVehicle ??= Vehicles.FirstOrDefault();

        if (SelectedVehicle != null && string.IsNullOrWhiteSpace(Input.VehicleSlug))
        {
            Input.VehicleSlug = SelectedVehicle.Slug;
            Input.VehicleName = SelectedVehicle.Name;
        }

        if (_currentUser.IsAuthenticated)
        {
            Input.FullName = _currentUser.UserName ?? string.Empty;
            Input.Email = _currentUser.Email ?? string.Empty;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Vehicles = _vehicleService.GetVehicles();

        if (!string.IsNullOrWhiteSpace(Input.VehicleSlug))
        {
            SelectedVehicle = Vehicles.FirstOrDefault(v =>
                string.Equals(v.Slug, Input.VehicleSlug, StringComparison.OrdinalIgnoreCase));
        }
        SelectedVehicle ??= Vehicles.FirstOrDefault();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var notes = $"[DỰ TOÁN TÀI CHÍNH / TRẢ GÓP]\n" +
                        $"- Xe quan tâm: {Input.VehicleName} (Giá: {Input.VehiclePrice})\n" +
                        $"- Khu vực đăng ký: {Input.City}\n" +
                        $"- Chi phí lăn bánh dự kiến: {Input.EstimatedOnRoadCost}\n" +
                        $"- Số tiền vay: {Input.LoanAmount} ({Input.DownPaymentPercent}% trả trước)\n" +
                        $"- Kỳ hạn vay: {Input.LoanTermMonths} tháng ({Input.LoanTermMonths / 12} năm)\n" +
                        $"- Lãi suất tạm tính: {Input.InterestRate}%/năm\n" +
                        $"- Trả góp tháng đầu: {Input.MonthlyPaymentFirst}\n" +
                        $"- Ghi chú khách: {Input.Notes}";

            var request = new ConsultationRequestEntity
            {
                FullName = Input.FullName.Trim(),
                Phone = Input.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
                UserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
                VehicleId = SelectedVehicle?.Id,
                Budget = Input.LoanAmount,
                ConsultationMethod = "GÓI VAY & LĂN BÁNH",
                Message = notes,
                CallbackPreference = "Điện thoại hoặc Zalo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "NEW"
            };

            var created = await _consultationService.CreateAsync(request);
            ConsultationCode = $"FIN-{created.Id:D6}";
            Submitted = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Loi khi tao yeu cau tu van tai chinh tra gop");
            ModelState.AddModelError(string.Empty, "Không thể gửi yêu cầu lúc này. Vui lòng liên hệ hotline.");
        }

        return Page();
    }

    public sealed class FinanceQuoteInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public string VehicleSlug { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string VehiclePrice { get; set; } = string.Empty;

        public string City { get; set; } = "Hà Nội";
        public string EstimatedOnRoadCost { get; set; } = string.Empty;
        public string DownPaymentPercent { get; set; } = "20";
        public string LoanAmount { get; set; } = string.Empty;
        public int LoanTermMonths { get; set; } = 60;
        public string InterestRate { get; set; } = "8.5";
        public string MonthlyPaymentFirst { get; set; } = string.Empty;

        [Display(Name = "Ghi chú thêm")]
        public string? Notes { get; set; }
    }
}
