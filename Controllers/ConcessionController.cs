using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System.Security.Claims;

namespace MovieManagement.Controllers
{
    public class ConcessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ConcessionController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index(int? showtimeId = null, string? selectedSeats = null, string? returnUrl = null)
        {
            var viewModel = new ConcessionViewModel
            {
                Foods = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Food && c.IsAvailable)
                    .ToListAsync(),
                    
                Drinks = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Drink && c.IsAvailable)
                    .ToListAsync(),
                    
                Combos = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Combo && c.IsAvailable)
                    .ToListAsync(),
                
                ShowtimeId = showtimeId,
                SelectedSeats = selectedSeats,
                IsComboWithTicket = showtimeId.HasValue && !string.IsNullOrEmpty(selectedSeats),
                ReturnUrl = returnUrl ?? "/"
            };
            
            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetConcessions()
        {
            var result = new
            {
                Foods = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Food && c.IsAvailable)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Price,
                        Type = (int)c.Type,
                        c.ImagePath,
                        c.IsAvailable
                    })
                    .ToListAsync(),
                    
                Drinks = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Drink && c.IsAvailable)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Price,
                        Type = (int)c.Type,
                        c.ImagePath,
                        c.IsAvailable
                    })
                    .ToListAsync(),
                    
                Combos = await _context.Concessions
                    .Where(c => c.Type == ConcessionType.Combo && c.IsAvailable)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Price,
                        Type = (int)c.Type,
                        c.ImagePath,
                        c.IsAvailable
                    })
                    .ToListAsync()
            };
            
            return Json(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddToCart(int[] itemIds, int[] quantities, int? showtimeId = null, string? selectedSeats = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Concession") });
            }              // Kiểm tra nếu có showtime + seats (mua vé) hoặc có concession items
            bool hasTickets = showtimeId.HasValue && !string.IsNullOrEmpty(selectedSeats);
            bool hasConcessions = itemIds != null && itemIds.Length > 0 && quantities != null && quantities.Length > 0 && itemIds.Length == quantities.Length;
            
            // Kiểm tra xem có thực sự chọn concession items không (quantity > 0)
            if (hasConcessions)
            {
                hasConcessions = quantities.Any(q => q > 0);
            }
            
            if (!hasTickets && !hasConcessions)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một vé hoặc sản phẩm bắp nước";
                return RedirectToAction("Index");
            }
            
            if (hasConcessions && itemIds.Length != quantities.Length)
            {
                TempData["Error"] = "Dữ liệu sản phẩm không hợp lệ";
                return RedirectToAction("Index");
            }
            
            // Tìm hoặc tạo giỏ hàng cho user
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
                
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            // Xác định nếu đây là một combo với vé
            bool isComboWithTicket = showtimeId.HasValue && !string.IsNullOrEmpty(selectedSeats);
            
            // Nếu là combo với vé, trước tiên chúng ta xử lý vé (giống như trong CartController.AddToCart)
            if (isComboWithTicket)
            {
                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .FirstOrDefaultAsync(s => s.Id == showtimeId);
                    
                if (showtime == null)
                {
                    TempData["Error"] = "Không tìm thấy suất chiếu";
                    return RedirectToAction("Index", "Movie");
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
                    TempData["Error"] = "Không tìm thấy thông tin ghế";
                    return RedirectToAction("Index", "Movie");
                }
                
                // Calculate total seat amount
                var totalTicketAmount = seats.Sum(s => s.Price);
                
                // Format seat names
                var seatNames = string.Join(", ", seats.Select(s => $"{s.Row}{s.Number}"));
                
                // Create cart item for tickets
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ShowtimeId = showtime.Id,
                    SelectedSeats = selectedSeats,
                    SeatNames = seatNames,
                    TotalAmount = totalTicketAmount,
                    Status = "Pending",
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                
                cart.Items.Add(cartItem);
                await _context.SaveChangesAsync();
                  // Create concession items and add to the same cart item (if any)
                decimal totalConcessionAmount = 0;
                
                if (hasConcessions)
                {
                    for (int i = 0; i < itemIds.Length; i++)
                    {
                        if (quantities[i] <= 0) continue;
                    
                    var concession = await _context.Concessions.FindAsync(itemIds[i]);
                    if (concession == null) continue;
                    
                    var itemPrice = concession.Price * quantities[i];
                    
                    // Apply 5% discount for combo with ticket
                    var discount = itemPrice * 0.05M;
                    
                    var concessionItem = new ConcessionItem
                    {
                        CartItemId = cartItem.Id,
                        ConcessionId = concession.Id,
                        Quantity = quantities[i],
                        Price = concession.Price,
                        Discount = discount,
                        IsComboItem = true
                    };
                    
                    totalConcessionAmount += (itemPrice - discount);
                          _context.ConcessionItems.Add(concessionItem);
                    }
                }
                
                // Update cart item total amount to include concessions (if any)
                cartItem.TotalAmount += totalConcessionAmount;
                
                await _context.SaveChangesAsync();
                
                if (hasConcessions)
                {
                    TempData["Success"] = "Thêm vé và đồ ăn vào giỏ hàng thành công!";
                }
                else
                {
                    TempData["Success"] = "Thêm vé vào giỏ hàng thành công!";
                }
            }
            else
            {
                // Create a new cart item for concession only (no ticket)
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ShowtimeId = null,  // No showtime for concession only
                    Status = "Pending",
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    TotalAmount = 0  // Will update after adding items
                };
                
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
                
                decimal totalAmount = 0;
                
                // Add each concession item
                for (int i = 0; i < itemIds.Length; i++)
                {
                    if (quantities[i] <= 0) continue;
                    
                    var concession = await _context.Concessions.FindAsync(itemIds[i]);
                    if (concession == null) continue;
                    
                    var itemPrice = concession.Price * quantities[i];
                    
                    var concessionItem = new ConcessionItem
                    {
                        CartItemId = cartItem.Id,
                        ConcessionId = concession.Id,
                        Quantity = quantities[i],
                        Price = concession.Price,
                        IsComboItem = false
                    };
                    
                    totalAmount += itemPrice;
                    
                    _context.ConcessionItems.Add(concessionItem);
                }
                
                cartItem.TotalAmount = totalAmount;
                cartItem.SeatNames = "Chỉ đồ ăn, nước uống";
                
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Thêm đồ ăn, nước uống vào giỏ hàng thành công!";
            }
            
            return RedirectToAction("Index", "Cart");
        }
    }
}
