using System.ComponentModel.DataAnnotations;
using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages;

public sealed class TestDriveModel(AutoHubDbContext db, ICurrentUser currentUser) : PageModel
{
    public static readonly string[] TimeSlots =
    [
        "08:00 - 10:00",
        "10:00 - 12:00",
        "13:00 - 15:00",
        "15:00 - 17:00",
    ];

    [BindProperty]
    public TestDriveInput Input { get; set; } = new();

    public IReadOnlyList<VehicleOption> VehicleOptions { get; private set; } = [];
    public bool Submitted { get; private set; }
    public string BookingReference { get; private set; } = string.Empty;

    public async Task OnGetAsync(string? vehicle)
    {
        await LoadVehicleOptionsAsync();

        if (!string.IsNullOrWhiteSpace(vehicle))
        {
            var match = VehicleOptions.FirstOrDefault(v => v.Slug == vehicle);
            if (match is not null)
            {
                Input.VariantId = match.Id;
            }
        }

        var firstSlot = DateOnly.FromDateTime(DateTime.Today);
        do
        {
            firstSlot = firstSlot.AddDays(1);
        }
        while (firstSlot.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
        Input.PreferredDate = firstSlot;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadVehicleOptionsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.PreferredDate < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(Input.PreferredDate), "Ngày lái thử không thể ở quá khứ.");
            return Page();
        }

        if (Input.PreferredDate > DateOnly.FromDateTime(DateTime.Today.AddDays(60)))
        {
            ModelState.AddModelError(nameof(Input.PreferredDate), "Chỉ nhận lịch trong vòng 60 ngày tới.");
            return Page();
        }

        if (Input.TimeSlots?.Length != 1)
        {
            ModelState.AddModelError(nameof(Input.TimeSlots), "Vui lòng chọn một khung giờ.");
            return Page();
        }
        Input.TimeSlot = Input.TimeSlots[0];

        if (Input.VariantId.HasValue &&
            !await db.VehicleVariants.AnyAsync(v => v.VariantId == Input.VariantId.Value, cancellationToken))
        {
            ModelState.AddModelError(nameof(Input.VariantId), "Xe đã chọn không tồn tại trong hệ thống.");
            return Page();
        }

        var booking = new TestDriveBookingEntity
        {
            UserId = currentUser.UserId,
            FullName = Input.FullName.Trim(),
            Phone = Input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
            VariantId = Input.VariantId,
            PreferredDate = Input.PreferredDate,
            TimeSlot = Input.TimeSlot,
            Notes = string.IsNullOrWhiteSpace(Input.Message) ? null : Input.Message.Trim(),
            Status = "PENDING",
        };

        db.TestDriveBookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);

        BookingReference = $"TD-{booking.BookingId:D6}";
        Submitted = true;
        return Page();
    }

    private async Task LoadVehicleOptionsAsync(CancellationToken cancellationToken = default)
    {
        VehicleOptions = await db.VehicleVariants
            .AsNoTracking()
            .Where(v => v.IsAvailable && v.Status == "ACTIVE")
            .OrderBy(v => v.Model.Brand.Name)
            .ThenBy(v => v.Model.Name)
            .ThenBy(v => v.VariantName)
            .Select(v => new VehicleOption(
                v.VariantId,
                v.Slug,
                v.Model.Brand.Name + " " + v.Model.Name + " " + v.VariantName,
                "Từ " + v.BasePrice.ToString("N0") + "đ"))
            .ToListAsync(cancellationToken);
    }

    public sealed class TestDriveInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{8,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public int? VariantId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày lái thử.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày mong muốn")]
        public DateOnly PreferredDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khung giờ.")]
        public string[]? TimeSlots { get; set; }

        public string TimeSlot { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? Message { get; set; }
    }

    public sealed record VehicleOption(int Id, string Slug, string Name, string PriceLabel);
}
