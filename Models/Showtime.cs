using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models;

public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public DateTime ShowDateTime { get; set; }
    
    [Column(TypeName = "nvarchar(10)")]
    public string ScreenType { get; set; } = string.Empty; // 2D, 3D, IMAX
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    
    public virtual Movie Movie { get; set; } = null!;
    public virtual Theater Theater { get; set; } = null!;
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Theater
{
    public int Id { get; set; }
    
    [Column(TypeName = "nvarchar(100)")]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(200)")]
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

