using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }        // GET: Admin/Ticket
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // Get all tickets including transfers
            var cartItems = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .Include(ci => ci.TransferredFromUser)
                .Where(ci => ci.Status == "Completed" && ci.Showtime != null)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            // Get all tickets that are pending payment
            var pendingItems = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .Where(ci => (ci.Status == "Pending" || ci.Status == "Processing") && ci.Showtime != null)
                .OrderByDescending(ci => ci.CreatedAt)
                .ToListAsync();

            // Combine both lists and filter out any items with null Showtime or Movie
            var allTickets = cartItems.Concat(pendingItems)
                .Where(ci => ci.Showtime?.Movie != null)
                .ToList();

            return View(allTickets);
        }

        // GET: Admin/Ticket/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .Include(ci => ci.TransferredFromUser)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
            {
                return NotFound();
            }

            // Get seats associated with the cart item
            var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .ToListAsync();

            ViewBag.Seats = seats;

            return View(cartItem);
        }

        // POST: Admin/Ticket/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.User)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (cartItem.Status != "Processing" && cartItem.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Vé không ở trạng thái chờ thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Update status to Completed without changing the payment method
                cartItem.Status = "Completed";
                // Keep the original payment method that user selected (vnpay, card, momo, etc.)
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã xác nhận thanh toán vé thành công qua {cartItem.PaymentMethod}!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận thanh toán: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Ticket/TransferTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferTicket(int cartItemId, string customerId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.User)
                .Include(ci => ci.Cart)
                .Include(ci => ci.Showtime)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé!" });
            }

            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
            }

            try
            {
                // Store the current user as the transferrer
                cartItem.TransferredFromUserId = cartItem.UserId;
                cartItem.TransferredFromUser = cartItem.User;

                // Get or create cart for the new user
                var customerCart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == customerId);

                if (customerCart == null)
                {
                    customerCart = new Cart
                    {
                        UserId = customerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Carts.Add(customerCart);
                    await _context.SaveChangesAsync();
                }

                // Update the ticket ownership
                cartItem.UserId = customerId;
                cartItem.User = customer;
                cartItem.CartId = customerCart.Id;
                cartItem.Cart = customerCart;
                cartItem.Status = "Completed";
                
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Đã chuyển vé thành công cho {customer.Email}" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi chuyển vé: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var customers = await _userManager.Users
                .Where(u => !_userManager.IsInRoleAsync(u, "Admin").Result &&
                           (u.FirstName.Contains(term) ||
                            u.LastName.Contains(term) ||
                            u.Email.Contains(term) ||
                            u.PhoneNumber.Contains(term)))
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FirstName + " " + u.LastName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber
                })
                .Take(10)
                .ToListAsync();

            return Json(customers);
        }

        // POST: Admin/Ticket/DeleteTicket/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                .Include(ci => ci.TransferredFromUser)
                .FirstOrDefaultAsync(ci => ci.Id == id);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vé!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Nếu vé đã hoàn thành hoặc đã chuyển, cần giải phóng các ghế đã đặt
                if (cartItem.Status == "Completed" || cartItem.TransferredFromUser != null)
                {
                    var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    var seats = await _context.Seats
                        .Where(s => seatIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var seat in seats)
                    {
                        seat.IsAvailable = true;
                        seat.Status = "Available";
                    }

                    // Clear selected seats after freeing them
                    cartItem.SelectedSeats = "";
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa vé thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa vé: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy vé!";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cho phép các trạng thái hợp lệ
            var validStatuses = new[] { "Pending", "Processing", "Completed" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            cartItem.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái vé!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Reset seat status when payment fails or is cancelled
        /// </summary>
        private async Task ResetSeatStatus(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem != null && !string.IsNullOrEmpty(cartItem.SelectedSeats))
                {
                    var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    var seats = await _context.Seats
                        .Where(s => seatIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var seat in seats)
                    {
                        seat.Status = "Available";
                        seat.IsAvailable = true;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
        }

        /// <summary>
        /// Auto-release seats that have been in processing status for too long (Admin only)
        /// </summary>
        [HttpGet, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoReleaseSeats()
        {
            try
            {
                // Find cart items that have been in processing status for more than 1 phút
                var cutoffTime = DateTime.UtcNow.AddMinutes(-1);
                var processingItems = await _context.CartItems
                    .Where(ci => ci.Status == "Processing" && ci.CreatedAt < cutoffTime)
                    .ToListAsync();

                int releasedCount = 0;
                foreach (var item in processingItems)
                {
                    await ResetSeatStatus(item.Id);
                    item.Status = "Cancelled";
                    releasedCount++;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã tự động giải phóng {releasedCount} ghế đã quá thời gian xử lý";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tự động giải phóng ghế";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Cancel payment and release seats (Admin only)
        /// </summary>
        [HttpGet, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPayment(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.User)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem == null)
                {
                    return NotFound();
                }

                // Reset seat status
                await ResetSeatStatus(cartItemId);

                // Update cart item status
                cartItem.Status = "Cancelled";
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã hủy thanh toán cho đơn hàng #{cartItemId} và giải phóng ghế";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi hủy thanh toán";
                return RedirectToAction("Index");
            }
        }
    }
} 