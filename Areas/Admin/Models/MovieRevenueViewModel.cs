using MovieManagement.Models;

namespace MovieManagement.Areas.Admin.Models
{
    public class MovieRevenueViewModel
    {
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
        public string? MovieTitle { get; set; }
        public int TicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
} 