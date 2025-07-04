using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Models;
using MovieManagement.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MovieManagement.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task UpdateCartItemCount(string userId)
    {
        var count = await _context.CartItems
            .Include(ci => ci.Cart)
            .Where(ci => ci.Cart.UserId == userId && ci.Status == "Pending")
            .CountAsync();
        HttpContext.Session.SetString("CartItemCount", count.ToString());
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Redirect("/Identity/Account/Login");
        }        // Get user's cart with all items
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Load pending and processing items separately with explicit includes
        var pendingItems = await _context.CartItems
            .Include(i => i.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(i => i.ConcessionItems)
                .ThenInclude(ci => ci.Concession)
            .Where(i => i.CartId == cart.Id && (i.Status == "Pending" || i.Status == "Processing"))
            .ToListAsync();

        cart.Items = pendingItems;// Get all completed tickets for this user
        var completedTickets = await _context.CartItems
            .Include(ci => ci.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(ci => ci.TransferredFromUser)
            .Include(ci => ci.ConcessionItems)
                .ThenInclude(ci => ci.Concession)
            .Where(ci => ci.UserId == userId && ci.Status == "Completed")
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync();

        // Initialize Items collection if null
        if (cart.Items == null)
        {
            cart.Items = new List<CartItem>();
        }

        // Add completed tickets to cart items
        foreach (var ticket in completedTickets)
        {
            if (!cart.Items.Any(i => i.Id == ticket.Id))
            {
                cart.Items.Add(ticket);
            }
        }        // Load seat information for each cart item
        foreach (var item in cart.Items)
        {
            List<Seat> seats = new List<Seat>();
            
            // Check if this item has tickets (showtime and seats)
            if (!string.IsNullOrEmpty(item.SelectedSeats))
            {
                var seatIds = item.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (seatIds.Any())
                {
                    seats = await _context.Seats
                        .Where(s => seatIds.Contains(s.Id))
                        .OrderBy(s => s.Row)
                        .ThenBy(s => s.Number)
                        .ToListAsync();

                    // Format seat names (e.g., "A1, A2, B1")
                    item.SeatNames = string.Join(", ", seats.Select(s => $"{s.Row}{s.Number}"));

                    // Only update TotalAmount if it's currently 0 or if there are no concessions
                    // Don't overwrite TotalAmount that already includes concessions
                    var seatTotalPrice = seats.Sum(s => s.Price);
                    if (item.TotalAmount == 0 || (item.ConcessionItems == null || !item.ConcessionItems.Any()))
                    {
                        item.TotalAmount = seatTotalPrice;
                    }
                }
            }
            else
            {
                // This is a concession-only item
                if (string.IsNullOrEmpty(item.SeatNames))
                {
                    item.SeatNames = "Chỉ đồ ăn, nước uống";
                }
                
                // Calculate total for concession-only items if not already set
                if (item.TotalAmount == 0 && item.ConcessionItems != null && item.ConcessionItems.Any())
                {
                    item.TotalAmount = item.ConcessionItems.Sum(ci => (ci.Price * ci.Quantity) - ci.Discount);
                }
            }
            
            // Clear and update the Seats collection
            if (item.Seats == null)
            {
                item.Seats = new List<Seat>();
            }
            item.Seats.Clear();
            foreach (var seat in seats)
            {
                item.Seats.Add(seat);
            }            // Debug information
            System.Diagnostics.Debug.WriteLine($"CartItem {item.Id}:");
            System.Diagnostics.Debug.WriteLine($"  TotalAmount: {item.TotalAmount}");
            System.Diagnostics.Debug.WriteLine($"  SeatNames: {item.SeatNames}");
            System.Diagnostics.Debug.WriteLine($"  SelectedSeats: {item.SelectedSeats}");
            System.Diagnostics.Debug.WriteLine($"  ShowtimeId: {item.ShowtimeId}");
            System.Diagnostics.Debug.WriteLine($"  ConcessionItems count: {item.ConcessionItems?.Count ?? 0}");
            if (item.ConcessionItems != null)
            {
                foreach (var concession in item.ConcessionItems)
                {
                    System.Diagnostics.Debug.WriteLine($"  Concession: {concession.Concession?.Name} - Price: {concession.Price} x {concession.Quantity} = {concession.Price * concession.Quantity}, Discount: {concession.Discount}, Total: {concession.TotalPrice}");
                }
                System.Diagnostics.Debug.WriteLine($"  Total Concession After Discount: {item.ConcessionItems.Sum(ci => ci.TotalPrice)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  ConcessionItems is null");
            }
        }

        // Sort all items by creation date
        cart.Items = cart.Items.OrderByDescending(i => i.CreatedAt).ToList();

        await UpdateCartItemCount(userId);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int showtimeId, string selectedSeats)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
        }

        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == showtimeId);

        if (showtime == null)
        {
            return Json(new { success = false, message = "Không tìm thấy suất chiếu" });
        }

        // Get seat IDs and load seat information
        var seatIds = selectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var seats = await _context.Seats
            .Where(s => seatIds.Contains(s.Id))
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Number)
            .ToListAsync();

        if (!seats.Any())
        {
            return Json(new { success = false, message = "Không tìm thấy thông tin ghế" });
        }

        // Calculate total amount and format seat names
        var totalAmount = seats.Sum(s => s.Price);
        var seatNames = string.Join(", ", seats.Select(s => $"{s.Row}{s.Number}"));

        var cartItem = new CartItem
        {
            CartId = cart.Id,
            ShowtimeId = showtimeId,
            SelectedSeats = selectedSeats,
            SeatNames = seatNames,
            TotalAmount = totalAmount,
            Status = "Pending",
            UserId = userId,
            Seats = seats.ToList() // Gán danh sách ghế vào CartItem
        };

        cart.Items.Add(cartItem);
        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        await UpdateCartItemCount(userId);

        return Json(new { success = true, message = "Đã thêm vào giỏ hàng thành công" });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng trong giỏ hàng" });
            }

            if (cartItem.Status != "Pending")
            {
                return Json(new { success = false, message = "Chỉ có thể xóa đơn hàng chưa thanh toán" });
            }

            // Get movie title for message
            string movieTitle = cartItem.Showtime?.Movie?.Title ?? "đơn hàng";

            // Get seat IDs and update their status
            var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();

            // Update seats status to Available
            foreach (var seat in seats)
            {
                seat.IsAvailable = true;
                seat.Status = "Available";
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            await UpdateCartItemCount(userId);

            return Json(new { 
                success = true, 
                message = $"Đã xóa {movieTitle} khỏi giỏ hàng thành công",
                cartItemId = cartItemId
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi khi xóa đơn hàng: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int cartItemId, string status)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Please login to update cart" });
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Item not found in cart" });
        }

        cartItem.Status = status;
        await _context.SaveChangesAsync();
        await UpdateCartItemCount(userId);

        return Json(new { success = true, message = "Status updated successfully" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TransferToCustomer(int cartItemId, string customerId)
    {
        try
        {
            // Load cart item with all necessary navigation properties
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.User)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin vé" });
            }

            // Kiểm tra xem khách hàng có tồn tại không
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
            }

            // Get or create cart for the customer
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

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Store the current user as the transferrer
                    cartItem.TransferredFromUserId = cartItem.UserId;
                    cartItem.TransferredFromUser = cartItem.User;

                    // Update cart item ownership
                    cartItem.UserId = customerId;
                    cartItem.User = customer;
                    cartItem.CartId = customerCart.Id;
                    cartItem.Cart = customerCart;
                    cartItem.Status = "Completed";

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { 
                        success = true, 
                        message = $"Đã chuyển vé thành công cho {customer.Email}",
                        cartItemId = cartItem.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Lỗi khi lưu thay đổi vào database", ex);
                }
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi khi chuyển vé: {ex.Message}" });
        }    }
}