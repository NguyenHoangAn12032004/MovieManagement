using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieManagement.Areas.StaffArea.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách vé cần xử lý
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .Where(ci => ci.Status == "Pending" || ci.Status == "Processing")
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        // Chi tiết vé
        public async Task<IActionResult> Details(int id)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }

        // Xác nhận thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Status = "Completed";
            cartItem.PaymentMethod = "Tiền mặt";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xác nhận thanh toán thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Hủy vé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Status = "Cancelled";

            // Reset seat status if applicable
            if (cartItem.ShowtimeId.HasValue && !string.IsNullOrEmpty(cartItem.SelectedSeats))
            {
                var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToList();

                var seats = await _context.Seats
                    .Where(s => seatIds.Contains(s.Id))
                    .ToListAsync();

                foreach (var seat in seats)
                {
                    seat.IsAvailable = true;
                    seat.Status = "Available";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy vé thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
