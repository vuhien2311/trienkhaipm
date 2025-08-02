using System.ComponentModel.DataAnnotations;

namespace WebBanHangOnline.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone]
        public string PhoneNumber { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ tôi?")]
        public bool RememberMe { get; set; }
    }
}