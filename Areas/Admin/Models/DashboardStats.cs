using System;

namespace MovieManagement.Areas.Admin.Models
{
    /// <summary>
    /// Dashboard statistics following cinema industry standards
    /// Separates ticket revenue from concession revenue for accurate reporting
    /// </summary>
    public class DashboardStats
    {
        // Revenue Statistics
        public decimal TotalRevenue { get; set; }        // Total revenue (tickets + concessions)
        public decimal TicketRevenue { get; set; }       // Pure ticket sales revenue
        public decimal ConcessionRevenue { get; set; }   // Pure concession sales revenue
        
        // Performance Metrics
        public decimal AttachRate { get; set; }          // % of ticket buyers who also bought concessions
        public decimal ConcessionMargin { get; set; }    // Concession revenue percentage of total
        
        // Volume Statistics
        public int TotalTicketsSold { get; set; }
        public int TotalConcessionsSold { get; set; }
        public int UniqueCustomers { get; set; }
        
        // Operational Statistics
        public int TotalMovies { get; set; }
        public int TotalUsers { get; set; }
        
        // Helper Properties for Display
        public string AttachRateDisplay => $"{AttachRate:F1}%";
        public string ConcessionMarginDisplay => $"{ConcessionMargin:F1}%";
        public string TicketRevenueDisplay => TicketRevenue.ToString("N0") + " ₫";
        public string ConcessionRevenueDisplay => ConcessionRevenue.ToString("N0") + " ₫";
        public string TotalRevenueDisplay => TotalRevenue.ToString("N0") + " ₫";    }
}
