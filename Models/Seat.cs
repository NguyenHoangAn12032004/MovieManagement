using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class Seat
    {
        public int Id { get; set; }
        
        [Column(TypeName = "nvarchar(2)")]
        public string Row { get; set; } = string.Empty;
        
        public int Number { get; set; }
        
        [Column(TypeName = "nvarchar(20)")]
        public string Type { get; set; } = string.Empty; // Standard, VIP, Couple
        
        public bool IsAvailable { get; set; }
        
        [Column(TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "Available"; // Available, Damaged, Cleaning
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public int? BookingId { get; set; }
        public int ShowtimeId { get; set; }
        public virtual Booking? Booking { get; set; }
        public virtual Showtime Showtime { get; set; } = null!;
    }
} 