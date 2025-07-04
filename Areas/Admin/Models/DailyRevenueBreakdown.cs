using System;

namespace MovieManagement.Areas.Admin.Models
{
    /// <summary>
    /// Represents daily revenue breakdown separating ticket and concession revenue
    /// Following cinema industry standards for accurate financial reporting
    /// </summary>
    public class DailyRevenueBreakdown
    {
        public DateTime Date { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal ConcessionRevenue { get; set; }
        public decimal TotalRevenue => TicketRevenue + ConcessionRevenue;
        public decimal ConcessionMargin => TotalRevenue > 0 ? (ConcessionRevenue / TotalRevenue * 100) : 0;
    }
}
