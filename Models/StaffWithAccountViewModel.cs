using System;
using System.ComponentModel.DataAnnotations;

namespace MovieManagement.Models
{
    public class StaffWithAccountViewModel
    {
        // Staff information
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Lương theo giờ là bắt buộc")]
        [Display(Name = "Lương theo giờ")]
        [Range(0, 100000, ErrorMessage = "Lương theo giờ phải từ 0 đến 100000")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Thưởng")]
        public decimal Bonus { get; set; } = 0;

        [Display(Name = "Phạt")]
        public decimal Penalty { get; set; } = 0;

        [Required(ErrorMessage = "Vị trí là bắt buộc")]
        [Display(Name = "Vị trí")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        // Account information
        [Display(Name = "Tạo tài khoản đăng nhập")]
        public bool CreateAccount { get; set; } = false;

        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Họ")]
        public string FirstName { get; set; }

        [Display(Name = "Tên")]
        public string LastName { get; set; }
    }
}
