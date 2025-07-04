using MovieManagement.Models;
using System.Collections.Generic;

namespace MovieManagement.Areas.StaffArea.Models
{
    public class StaffDashboardViewModel
    {
        public int TodayTickets { get; set; }
        public decimal TodayRevenue { get; set; }
        public int PendingTickets { get; set; }
        public MovieManagement.Models.Staff StaffInfo { get; set; }
    }

    public class StaffTicketViewModel
    {
        public CartItem CartItem { get; set; }
        public List<Seat> Seats { get; set; }
        public List<ConcessionItem> ConcessionItems { get; set; }
    }
}
