using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

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

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; }
    }
} 