using System.ComponentModel.DataAnnotations;

namespace WebBanHangOnline.ViewModels
{
    public class ManageAccountViewModel
    {
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        // Email không cho phép chỉnh sửa
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? ConfirmPassword { get; set; }
    }
}
