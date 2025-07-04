using System;
using System.Collections.Generic;

namespace MovieManagement.Models
{
    public class SeatViewModel
    {
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowtimeDateTime { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();
    }
} 