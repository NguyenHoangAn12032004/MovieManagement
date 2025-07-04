using System.ComponentModel.DataAnnotations;
using Staff = MovieManagement.Models.Staff;

namespace MovieManagement.Areas.Admin.Models
{
    public class CreateStaffAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        [Display(Name = "Nhân viên")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Tài khoản hoạt động")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public Staff? Staff { get; set; }
        public List<Staff> AvailableStaff { get; set; } = new List<Staff>();
    }

    public class EditStaffAccountViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Tài khoản hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Vai trò")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        // Navigation property
        public Staff Staff { get; set; } = new Staff();
    }

    public class StaffAccountListViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public Staff Staff { get; set; } = new Staff();
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string StaffName { get; set; } = string.Empty;
    }
}
