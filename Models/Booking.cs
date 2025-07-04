using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MovieManagement.Models;

public class Booking
{
    [Key]
    public int Id { get; set; }

    public int? ShowtimeId { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SelectedSeats { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Status { get; set; } = "Pending";

    [Column(TypeName = "nvarchar(50)")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(450)")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime BookingTime { get; set; } = DateTime.UtcNow;

    public DateTime BookingDate { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("ShowtimeId")]
    public virtual Showtime? Showtime { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}

public enum BookingStatus
{
    Pending,
    Paid,
    Cancelled
}




