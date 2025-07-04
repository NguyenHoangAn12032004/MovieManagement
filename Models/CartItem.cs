using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MovieManagement.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }        public int? ShowtimeId { get; set; }        [Column(TypeName = "nvarchar(max)")]
        public string? SelectedSeats { get; set; } = string.Empty; // Comma-separated seat IDs

        [Column(TypeName = "nvarchar(max)")]
        public string SeatNames { get; set; } = string.Empty; // Formatted seat names (e.g., "A1, A2, B1")

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        [Column(TypeName = "nvarchar(50)")]
        public string PaymentMethod { get; set; } = string.Empty; // cash, momo, vnpay, card

        [Required]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(450)")]
        public string? TransferredFromUserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CartId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Cart Cart { get; set; } = null!;        [ForeignKey("ShowtimeId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Showtime? Showtime { get; set; } = null;

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("TransferredFromUserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser? TransferredFromUser { get; set; }        // Virtual collection for seats
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        
        // Virtual collection for concession items
        public virtual ICollection<ConcessionItem> ConcessionItems { get; set; } = new List<ConcessionItem>();
    }
} 