using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class StaffSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        [Required]
        [Display(Name = "Ngày")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Ca làm việc")]
        public string Shift { get; set; } // Ca1, Ca2, Ca3

        [Display(Name = "Ghi chú")]
        public string Note { get; set; } = "";

        [Display(Name = "Trạng thái")]
        public bool IsApproved { get; set; } = false;
    }
} 