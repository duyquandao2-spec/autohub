using System.ComponentModel.DataAnnotations;
using AutoHub.Data;
using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public sealed class ContactModel(AutoHubDbContext db) : PageModel
{
    public static readonly (string Value, string Label)[] Subjects =
    [
        ("TƯ VẤN XE", "Tư vấn chọn xe"),
        ("BÁO GIÁ", "Yêu cầu báo giá"),
        ("HỢP TÁC", "Hợp tác kinh doanh"),
        ("KHÁC", "Câu hỏi khác"),
    ];

    [BindProperty]
    public ContactInput Input { get; set; } = new();

    public bool Submitted { get; private set; }
    public string MessageReference { get; private set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var message = new ContactMessageEntity
        {
            FullName = Input.FullName.Trim(),
            Phone = Input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
            Subject = Input.Subject,
            Content = Input.Content.Trim(),
        };

        db.ContactMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        MessageReference = $"CT-{message.MessageId:D6}";
        Submitted = true;
        return Page();
    }

    public sealed class ContactInput
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

        [Required(ErrorMessage = "Vui lòng chọn chủ đề.")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Nội dung từ 10 đến 2000 ký tự.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;
    }
}
