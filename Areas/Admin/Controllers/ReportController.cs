using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Areas.Admin.Models;
using MovieManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MovieRevenue()
        {
            var movieRevenue = await _context.CartItems
                .Where(c => c.Status == "Completed")
                .GroupBy(c => c.Showtime.Movie.Id)
                .Select(g => new MovieRevenueViewModel
                {
                    MovieId = g.Key,
                    Movie = g.First().Showtime.Movie,
                    MovieTitle = g.First().Showtime.Movie.Title,
                    TicketsSold = g.Count(),
                    TotalRevenue = g.Sum(c => c.TotalAmount)
                })
                .OrderByDescending(m => m.TotalRevenue)
                .ToListAsync();

            return View(movieRevenue);
        }
    }
} 