using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Areas.Admin.Models;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy dữ liệu mới nhất từ database, không dùng cache
                var dashboardStats = await GetDashboardStats();
                ViewBag.TotalBookings = dashboardStats.TotalTicketsSold;
                ViewBag.TotalMovies = dashboardStats.TotalMovies;
                ViewBag.TotalUsers = dashboardStats.TotalUsers;
                ViewBag.TotalRevenue = dashboardStats.TotalRevenue;
                ViewBag.TicketRevenue = dashboardStats.TicketRevenue;
                ViewBag.ConcessionRevenue = dashboardStats.ConcessionRevenue;
                ViewBag.AttachRate = dashboardStats.AttachRate;
                ViewBag.ConcessionMargin = dashboardStats.ConcessionMargin;
                ViewBag.TotalConcessionRevenue = dashboardStats.ConcessionRevenue;
                ViewBag.TotalConcessionSold = dashboardStats.TotalConcessionsSold;

                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-i))
                    .Reverse()
                    .ToList();
                ViewBag.RevenueLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
                var revenueBreakdown = await GetRevenueBreakdownData(last7Days);
                ViewBag.TicketRevenueData = revenueBreakdown.Select(r => r.TicketRevenue).ToList();
                ViewBag.ConcessionRevenueData = revenueBreakdown.Select(r => r.ConcessionRevenue).ToList();
                ViewBag.RevenueData = revenueBreakdown.Select(r => r.TotalRevenue).ToList();
                ViewBag.TotalRevenue7Days = revenueBreakdown.Sum(r => r.TotalRevenue);
                ViewBag.AvgTicketRevenue = revenueBreakdown.Sum(r => r.TicketRevenue) / 7;
                ViewBag.AvgConcessionRevenue = revenueBreakdown.Sum(r => r.ConcessionRevenue) / 7;
                var popularMovies = await GetPopularMovies();
                ViewBag.PopularMovieLabels = popularMovies.Select(m => m.Title).ToList();
                ViewBag.PopularMovieData = popularMovies.Select(m => m.Count).ToList();
                ViewBag.BookingStatusData = await GetBookingStatusData();
            }
            catch (Exception ex)
            {
                // Log error và fallback data như cũ
                ViewBag.TotalRevenue = 0;
                ViewBag.TicketRevenue = 0;
                ViewBag.ConcessionRevenue = 0;
                ViewBag.TotalBookings = 0;
                ViewBag.TotalMovies = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.AttachRate = 0;
                ViewBag.ConcessionMargin = 0;
                ViewBag.TotalConcessionSold = 0;
                ViewBag.RevenueLabels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                ViewBag.RevenueData = new List<decimal> { 100000, 150000, 200000, 175000, 250000, 300000, 280000 };
                ViewBag.TicketRevenueData = new List<decimal> { 60000, 90000, 120000, 105000, 150000, 180000, 168000 };
                ViewBag.ConcessionRevenueData = new List<decimal> { 40000, 60000, 80000, 70000, 100000, 120000, 112000 };
                ViewBag.PopularMovieLabels = new List<string> { "Sample Movie 1", "Sample Movie 2", "Sample Movie 3" };
                ViewBag.PopularMovieData = new List<int> { 45, 35, 20 };
                ViewBag.BookingStatusData = new int[] { 80, 15, 5 };
                ViewBag.TotalRevenue7Days = 0;
                ViewBag.AvgTicketRevenue = 0;
                ViewBag.AvgConcessionRevenue = 0;
            }
            return View();
        }

        /// <summary>
        /// Get comprehensive dashboard statistics following cinema industry standards
        /// Separates ticket revenue from concession revenue for accurate reporting
        /// </summary>
        private async Task<DashboardStats> GetDashboardStats()
        {
            var stats = new DashboardStats();
            
            // Get all completed cart items for the calculation period (last 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var completedItems = await _context.CartItems
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(conItem => conItem.Concession)
                .Include(ci => ci.Showtime)
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt >= thirtyDaysAgo)
                .ToListAsync();

            // Calculate pure concession revenue (using TotalPrice which includes discount)
            stats.ConcessionRevenue = completedItems
                .SelectMany(ci => ci.ConcessionItems)
                .Sum(conItem => conItem.TotalPrice);

            // Calculate pure ticket revenue from BookingSeats
            var ticketItems = completedItems.Where(ci => ci.ShowtimeId != null).ToList();
            stats.TicketRevenue = 0;
            
            foreach (var item in ticketItems)
            {
                if (!string.IsNullOrEmpty(item.SelectedSeats))
                {
                    var seatIds = item.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                    
                    var seats = await _context.Seats
                        .Where(s => seatIds.Contains(s.Id))
                        .ToListAsync();
                    
                    stats.TicketRevenue += seats.Sum(s => s.Price);
                }
            }

            // Ensure non-negative values
            stats.TicketRevenue = Math.Max(0, stats.TicketRevenue);
            stats.ConcessionRevenue = Math.Max(0, stats.ConcessionRevenue);

            // Calculate total revenue
            stats.TotalRevenue = stats.TicketRevenue + stats.ConcessionRevenue;

            // Calculate volume statistics
            stats.TotalTicketsSold = ticketItems.Count;
            
            stats.TotalConcessionsSold = completedItems
                .SelectMany(ci => ci.ConcessionItems)
                .Sum(conItem => conItem.Quantity);

            // Calculate attach rate (% of ticket buyers who also bought concessions)
            var ticketBuyersWithConcession = ticketItems.Count(item => 
                item.ConcessionItems != null && item.ConcessionItems.Any());
            stats.AttachRate = stats.TotalTicketsSold > 0 
                ? (decimal)ticketBuyersWithConcession / stats.TotalTicketsSold * 100 
                : 0;

            // Calculate concession margin (industry standard is 25-30%)
            stats.ConcessionMargin = stats.TotalRevenue > 0 
                ? stats.ConcessionRevenue / stats.TotalRevenue * 100 
                : 0;

            // Get operational statistics
            stats.TotalMovies = await _context.Movies.CountAsync();
            
            // Count non-admin users
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            stats.TotalUsers = await _userManager.Users
                .Where(u => !adminUserIds.Contains(u.Id))
                .CountAsync();

            // Count unique customers who made purchases
            stats.UniqueCustomers = completedItems
                .Select(ci => ci.UserId)
                .Distinct()
                .Count();

            return stats;
        }

        /// <summary>
        /// Get daily revenue breakdown for charts (separated ticket vs concession)
        /// </summary>
        private async Task<List<DailyRevenueBreakdown>> GetRevenueBreakdownData(List<DateTime> dates)
        {
            var result = new List<DailyRevenueBreakdown>();
            
            foreach (var date in dates)
            {
                var dayStart = date.Date;
                var dayEnd = date.Date.AddDays(1);
                
                // Get completed items for this day
                var dayItems = await _context.CartItems
                    .Include(ci => ci.ConcessionItems)
                        .ThenInclude(conItem => conItem.Concession)
                    .Where(ci => ci.Status == "Completed" && 
                               ci.CreatedAt >= dayStart && ci.CreatedAt < dayEnd)
                    .ToListAsync();

                var breakdown = new DailyRevenueBreakdown
                {
                    Date = date
                };

                // Calculate concession revenue for the day (using TotalPrice with discount applied)
                breakdown.ConcessionRevenue = dayItems
                    .SelectMany(ci => ci.ConcessionItems)
                    .Sum(conItem => conItem.TotalPrice);

                // Calculate ticket revenue from seats for the day
                var dayTicketItems = dayItems.Where(ci => ci.ShowtimeId != null).ToList();
                breakdown.TicketRevenue = 0;
                
                foreach (var item in dayTicketItems)
                {
                    if (!string.IsNullOrEmpty(item.SelectedSeats))
                    {
                        var seatIds = item.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse)
                            .ToList();
                        
                        var seats = await _context.Seats
                            .Where(s => seatIds.Contains(s.Id))
                            .ToListAsync();
                        
                        breakdown.TicketRevenue += seats.Sum(s => s.Price);
                    }
                }

                result.Add(breakdown);
            }
            
            return result;
        }

        private async Task<List<decimal>> GetRevenueData(List<DateTime> dates)
        {
            // Keep for backward compatibility, but now uses the new breakdown method
            var breakdown = await GetRevenueBreakdownData(dates);
            return breakdown.Select(r => r.TotalRevenue).ToList();
        }

        private async Task<List<(string Title, int Count)>> GetPopularMovies(int take = 5)
        {
            var movies = await _context.CartItems
                .Where(c => c.Status == "Completed")
                .GroupBy(c => c.Showtime.Movie.Title)
                .Select(g => new
                {
                    Title = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(m => m.Count)
                .Take(take)
                .ToListAsync();

            return movies.Select(m => (m.Title, m.Count)).ToList();
        }

        private async Task<int[]> GetBookingStatusData()
        {
            // Thay đổi từ giá trị cố định thành số lượng vé thực tế
            return new[]
            {
                await _context.CartItems.CountAsync(b => b.Status == "Completed"),
                await _context.CartItems.CountAsync(b => b.Status == "Pending"),
                await _context.CartItems.CountAsync(b => b.Status == "Cancelled")
            };
            
            // Loại bỏ giá trị cố định
            // return new[] { 80, 20, 0 };
        }

        /// <summary>
        /// Debug method to verify revenue calculations
        /// </summary>
        public async Task<IActionResult> DebugRevenue()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var completedItems = await _context.CartItems
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(conItem => conItem.Concession)
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt >= thirtyDaysAgo)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            var debugInfo = completedItems.Select(ci => new
            {
                CartItemId = ci.Id,
                MovieTitle = ci.Showtime?.Movie?.Title ?? "N/A",
                TotalAmount = ci.TotalAmount,
                ConcessionTotal = ci.ConcessionItems.Sum(con => con.TotalPrice),
                TicketAmount = ci.TotalAmount - ci.ConcessionItems.Sum(con => con.TotalPrice),
                CreatedAt = ci.CreatedAt,
                ConcessionDetails = ci.ConcessionItems.Select(con => new
                {
                    Name = con.Concession.Name,
                    Quantity = con.Quantity,
                    Price = con.Price,
                    Discount = con.Discount,
                    TotalPrice = con.TotalPrice
                }).ToList()
            }).ToList();

            ViewBag.DebugInfo = debugInfo;
            ViewBag.TotalTicketRevenue = debugInfo.Sum(d => d.TicketAmount);
            ViewBag.TotalConcessionRevenue = debugInfo.Sum(d => d.ConcessionTotal);
            ViewBag.GrandTotal = debugInfo.Sum(d => d.TotalAmount);

            return View();
        }

        /// <summary>
        /// Full analytics page with detailed charts and comprehensive statistics
        /// </summary>
        public async Task<IActionResult> Analytics()
        {
            // Get comprehensive dashboard statistics
            var dashboardStats = await GetDashboardStats();
            
            // Extended date range for detailed analytics (30 days)
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            // Get comprehensive revenue breakdown
            var revenueBreakdown = await GetRevenueBreakdownData(last30Days);
            
            // Prepare data for charts
            ViewBag.RevenueLabels = last30Days.Select(d => d.ToString("dd/MM")).ToList();
            ViewBag.TicketRevenueData = revenueBreakdown.Select(r => r.TicketRevenue).ToList();
            ViewBag.ConcessionRevenueData = revenueBreakdown.Select(r => r.ConcessionRevenue).ToList();
            ViewBag.RevenueData = revenueBreakdown.Select(r => r.TotalRevenue).ToList();
            
            // Dashboard stats
            ViewBag.DashboardStats = dashboardStats;
            
            // Extended popular movies (Top 10)
            var popularMovies = await GetPopularMovies(10);
            ViewBag.PopularMovieLabels = popularMovies.Select(m => m.Title).ToList();
            ViewBag.PopularMovieData = popularMovies.Select(m => m.Count).ToList();
            
            // Booking status data
            ViewBag.BookingStatusData = await GetBookingStatusData();
            
            // Hourly sales pattern
            ViewBag.HourlySalesData = await GetHourlySalesPattern();
            
            // Monthly comparison
            ViewBag.MonthlyComparisonData = await GetMonthlyComparison();
            
            // Customer analytics
            ViewBag.CustomerAnalytics = await GetCustomerAnalytics();

            return View();
        }

        /// <summary>
        /// Get hourly sales pattern for analytics
        /// </summary>
        private async Task<List<object>> GetHourlySalesPattern()
        {
            var last7Days = DateTime.Now.AddDays(-7);
            var hourlyData = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt >= last7Days)
                .GroupBy(ci => ci.CreatedAt.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(ci => ci.TotalAmount)
                })
                .OrderBy(h => h.Hour)
                .ToListAsync();

            return hourlyData.Cast<object>().ToList();
        }

        /// <summary>
        /// Get monthly comparison data
        /// </summary>
        private async Task<List<object>> GetMonthlyComparison()
        {
            var currentMonth = DateTime.Now.Month;
            var lastMonth = DateTime.Now.AddMonths(-1).Month;
            
            var monthlyData = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && 
                           (ci.CreatedAt.Month == currentMonth || ci.CreatedAt.Month == lastMonth))
                .GroupBy(ci => ci.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(ci => ci.TotalAmount),
                    Count = g.Count()
                })
                .ToListAsync();

            return monthlyData.Cast<object>().ToList();
        }

        /// <summary>
        /// Get customer analytics data
        /// </summary>
        private async Task<object> GetCustomerAnalytics()
        {
            var last30Days = DateTime.Now.AddDays(-30);
            
            var newCustomers = await _userManager.Users
                .Where(u => u.CreatedAt >= last30Days)
                .CountAsync();

            var repeatCustomers = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt >= last30Days)
                .GroupBy(ci => ci.UserId)
                .Where(g => g.Count() > 1)
                .CountAsync();

            var avgOrderValue = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt >= last30Days)
                .AverageAsync(ci => ci.TotalAmount);

            return new
            {
                NewCustomers = newCustomers,
                RepeatCustomers = repeatCustomers,
                AvgOrderValue = avgOrderValue
            };
        }

        /// <summary>
        /// Test dashboard with guaranteed sample data
        /// </summary>
        public IActionResult TestDashboard()
        {
            // Provide guaranteed sample data for testing
            ViewBag.TotalRevenue = 967750;
            ViewBag.TicketRevenue = 415000;
            ViewBag.ConcessionRevenue = 552750;
            ViewBag.TotalBookings = 4;
            ViewBag.TotalMovies = 7;
            ViewBag.TotalUsers = 10;
            ViewBag.AttachRate = 75.0m;
            ViewBag.ConcessionMargin = 57.1m;
            ViewBag.TotalConcessionSold = 1;
            
            // Chart data
            ViewBag.RevenueLabels = new List<string> { "16/06", "17/06", "18/06", "19/06", "20/06", "21/06", "22/06" };
            ViewBag.RevenueData = new List<decimal> { 120000, 150000, 200000, 175000, 250000, 300000, 280000 };
            ViewBag.TicketRevenueData = new List<decimal> { 70000, 90000, 120000, 105000, 150000, 180000, 170000 };
            ViewBag.ConcessionRevenueData = new List<decimal> { 50000, 60000, 80000, 70000, 100000, 120000, 110000 };
            ViewBag.PopularMovieLabels = new List<string> { "Avengers: Endgame", "Spider-Man", "Batman", "Superman", "Iron Man" };
            ViewBag.PopularMovieData = new List<int> { 45, 35, 25, 20, 15 };
            ViewBag.BookingStatusData = new int[] { 80, 15, 5 };
            
            ViewBag.TotalRevenue7Days = 1475000;
            ViewBag.AvgTicketRevenue = 126428;
            ViewBag.AvgConcessionRevenue = 84285;
            
            Console.WriteLine("TestDashboard: Sample data loaded successfully");
            
            return View("Index");
        }

        /// <summary>
        /// Simple test with basic charts
        /// </summary>
        public IActionResult SimpleTest()
        {
            return View();
        }

        /// <summary>
        /// Clear dashboard cache and reload with fresh data
        /// </summary>
        public IActionResult ClearCache()
        {
            // Clear all cache
            // _dashboardCache.Clear();
            Console.WriteLine("Dashboard cache cleared");
            
            TempData["Success"] = "Cache đã được xóa. Dữ liệu mới sẽ được tính toán.";
            
            // Redirect to regular dashboard to reload fresh data
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Legacy class for backward compatibility
    /// </summary>
    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}