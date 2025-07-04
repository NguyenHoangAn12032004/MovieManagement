using MovieManagement.Models;
using System;
using System.Collections.Generic;

namespace MovieManagement.Areas.Admin.Models
{
    public class AdminSeatViewModel
    {
        public List<Seat> Seats { get; set; } = new List<Seat>();
        public List<Theater> Theaters { get; set; } = new List<Theater>();
        public int? SelectedTheaterId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        
        // Statistics
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int MaintenanceSeats { get; set; }
        public int CleaningSeats { get; set; }
        public int BookedSeats { get; set; }
    }
    
    public class SeatDetailsViewModel
    {
        public Seat Seat { get; set; } = null!;
        public Theater Theater { get; set; } = null!;
        public List<BookingHistory> BookingHistory { get; set; } = new List<BookingHistory>();
    }
    
    public class BookingHistory
    {
        public int BookingId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowtimeDateTime { get; set; }
    }
} 