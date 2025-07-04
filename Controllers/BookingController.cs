using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Models;
using MovieManagement.Services;
using MovieManagement.Data;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Text;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace MovieTicketBooking.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IMovieService _movieService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(
        IMovieService movieService,
        ApplicationDbContext context,
        ILogger<BookingController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _movieService = movieService;
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int movieId)
    {
        try
        {
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            var allMovies = showingMovies.Concat(comingSoonMovies).ToList();
            
            var movie = allMovies.FirstOrDefault(m => m.Id == movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var showtimes = await _movieService.GetShowtimesAsync(movieId);
            ViewBag.Movie = movie;
            ViewBag.Showtimes = showtimes;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Booking/Index action");
            return View("Error");
        }
    }

    public async Task<IActionResult> Book(int showtimeId)
    {
        try
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            var seats = await _context.Seats
                .Where(s => s.ShowtimeId == showtimeId)
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .ToListAsync();

            ViewBag.Showtime = showtime;
            ViewBag.Seats = seats;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Booking/Book action");
            return View("Error");
        }
    }

    public async Task<IActionResult> SelectSeats(int showtimeId)
    {
        try {
            // Get the showtime with movie details
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                TempData["Error"] = "Suất chiếu không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Always get fresh seat data from the database with no tracking to avoid caching issues
            var seats = await _context.Seats
                .AsNoTracking()
                .Where(s => s.ShowtimeId == showtimeId)
                .ToListAsync();
                
            // If no seats exist, create them
            if (!seats.Any())
            {
                // Create new list for seats to be added
                var newSeats = new List<Seat>();
                string[] rows = { "A", "B", "C", "D", "E", "F", "G", "H" };
                
                foreach (var row in rows)
                {
                    for (int number = 1; number <= 12; number++)
                    {
                        var seatType = row switch
                        {
                            "A" or "B" => "Standard",
                            "C" or "D" or "E" or "F" => "VIP",
                            _ => "Couple" // G and H rows
                        };

                        var price = seatType switch
                        {
                            "VIP" => 80000M,
                            "Standard" => 50000M,
                            "Couple" => 95000M,
                            _ => 50000M
                        };

                        newSeats.Add(new Seat
                        {
                            Row = row,
                            Number = number,
                            Type = seatType,
                            IsAvailable = true,
                            Status = "Available",
                            Price = price,
                            ShowtimeId = showtimeId
                        });
                    }
                }

                // Save the seats to the database
                await _context.Seats.AddRangeAsync(newSeats);
                await _context.SaveChangesAsync();
                
                // Refresh seats list from database
                seats = await _context.Seats
                    .AsNoTracking()
                    .Where(s => s.ShowtimeId == showtimeId)
                    .ToListAsync();
            }

            // Log seat statuses for debugging
            foreach (var seat in seats.Where(s => s.Status != "Available"))
            {
                System.Diagnostics.Debug.WriteLine($"Seat {seat.Row}{seat.Number}: Status={seat.Status}, IsAvailable={seat.IsAvailable}");
            }
            
            var viewModel = new SeatViewModel
            {
                ShowtimeId = showtimeId,
                MovieTitle = showtime.Movie.Title,
                ShowtimeDateTime = showtime.ShowDateTime,
                Seats = seats
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SelectSeats: {ex.Message}");
            TempData["Error"] = "Có lỗi xảy ra khi tải thông tin ghế.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentConfirm(int cartItemId)
    {
        try
        {
            // Log the request
            System.Diagnostics.Debug.WriteLine($"PaymentConfirm called with cartItemId: {cartItemId}");

            // Get both Name and NameIdentifier claims to handle both cases
            var userId = User.Identity?.Name;
            var userIdFromClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userIdFromClaim))
            {
                TempData["Error"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account", new { area = "Identity", returnUrl = Url.Action("PaymentConfirm", "Booking", new { cartItemId }) });
            }

            // Log the user IDs
            System.Diagnostics.Debug.WriteLine($"User.Identity.Name: {userId}");
            System.Diagnostics.Debug.WriteLine($"NameIdentifier: {userIdFromClaim}");            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.Cart)
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt vé.";
                return RedirectToAction("Index", "Cart");
            }

            // Log the cart item details
            System.Diagnostics.Debug.WriteLine($"Cart Item found: ID={cartItem.Id}, ShowtimeId={cartItem.ShowtimeId}, Status={cartItem.Status}");
            System.Diagnostics.Debug.WriteLine($"Cart UserId: {cartItem.Cart.UserId}");

            // Check if either user ID matches
            if (cartItem.Cart.UserId != userId && cartItem.Cart.UserId != userIdFromClaim)
            {
                TempData["Error"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToAction("Index", "Cart");
            }            if (cartItem.Status != "Pending")
            {
                TempData["Error"] = "Đơn hàng này đã được xử lý.";
                return RedirectToAction("Index", "Cart");
            }

            // Check if this is a concession-only item (no showtime/seats)
            bool isConcessionOnly = cartItem.ShowtimeId == null || string.IsNullOrEmpty(cartItem.SelectedSeats);
            List<Seat> seats = new List<Seat>();

            if (!isConcessionOnly)
            {
                // Load seat information for ticket items
                var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                seats = await _context.Seats
                    .Where(s => seatIds.Contains(s.Id))
                    .ToListAsync();

                if (!seats.Any())
                {
                    TempData["Error"] = "Không tìm thấy thông tin ghế.";
                    return RedirectToAction("Index", "Cart");
                }
            }            // Update cart item with seat names if not set (only for ticket items)
            if (!isConcessionOnly && string.IsNullOrEmpty(cartItem.SeatNames))
            {
                cartItem.SeatNames = string.Join(", ", seats.Select(s => $"{s.Row}{s.Number}"));
                await _context.SaveChangesAsync();
            }

            // Cập nhật trạng thái ghế thành "đang xử lý" và đánh dấu là không khả dụng (chỉ với ticket items)
            if (!isConcessionOnly)
            {
                foreach (var seat in seats)
                {
                    seat.Status = "Processing"; // Đánh dấu là đang xử lý
                    seat.IsAvailable = false; // Đánh dấu là không khả dụng để tránh đặt lại
                }
                await _context.SaveChangesAsync();
            }

            var viewModel = new PaymentViewModel
            {
                CartItem = cartItem,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod { Id = "cash", Name = "Thanh toán tiền mặt", Description = "Thanh toán tại quầy khi nhận vé" },
                    new PaymentMethod { Id = "momo", Name = "MoMo", Description = "Thanh toán qua ví điện tử MoMo" },
                    new PaymentMethod { Id = "vnpay", Name = "VNPay", Description = "Thanh toán qua VNPay QR" },
                    new PaymentMethod { Id = "card", Name = "Thẻ tín dụng/ghi nợ", Description = "Visa, Mastercard, JCB" }
                }
            };

            // Log the view model
            System.Diagnostics.Debug.WriteLine("PaymentViewModel created successfully");

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error in PaymentConfirm: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            return RedirectToAction("Index", "Cart");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(int cartItemId, string paymentMethod)
    {
        try
        {
            _logger.LogInformation($"Processing payment for cart item {cartItemId} with method {paymentMethod}");

            // Get user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return RedirectToAction("Login", "Account");
            }            // Get cart item with related data
            var cartItem = await _context.CartItems
                .Include(c => c.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(c => c.Seats)
                .Include(c => c.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                _logger.LogWarning($"Cart item {cartItemId} not found for user {userId}");
                TempData["Error"] = "Không tìm thấy thông tin đặt vé";
                return RedirectToAction("Index", "Cart");
            }

            // Process payment based on method
            switch (paymentMethod.ToLower())
            {
                case "cash":
                    // For cash payment, create booking directly
                    return await ProcessCashPayment(cartItem);

                case "momo":
                    // Redirect to Momo payment
                    return await ProcessMomoPayment(cartItem);

                case "vnpay":
                    // Redirect to VNPay payment
                    return await ProcessVNPayPayment(cartItem);

                case "card":
                    // Process card payment
                    return await ProcessCardPayment(cartItem);

                default:
                    _logger.LogWarning($"Invalid payment method: {paymentMethod}");
                    TempData["Error"] = "Phương thức thanh toán không hợp lệ";
                    return RedirectToAction("PaymentConfirm", new { cartItemId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing payment for cart item {cartItemId}");
            TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán";
            return RedirectToAction("PaymentConfirm", new { cartItemId });
        }
    }    private async Task<IActionResult> ProcessCashPayment(CartItem cartItem)
    {
        try
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                // Check if this is a concession-only item
                bool isConcessionOnly = cartItem.ShowtimeId == null || string.IsNullOrEmpty(cartItem.SelectedSeats);

                if (!isConcessionOnly)
                {
                    // Process seat booking for ticket items
                    var selectedSeatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    // Lấy thông tin ghế đã chọn mà không cần kiểm tra IsAvailable
                    var selectedSeats = await _context.Seats
                        .Where(s => selectedSeatIds.Contains(s.Id))
                        .ToListAsync();

                    if (selectedSeats.Count != selectedSeatIds.Count)
                    {
                        TempData["Error"] = "Không tìm thấy thông tin ghế.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Đánh dấu ghế đã được thanh toán
                    foreach (var seat in selectedSeats)
                    {
                        seat.IsAvailable = false; // Mark as unavailable
                        seat.Status = "Booked"; // Ghế đã đặt thành công
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }            // Create booking
            var booking = new Booking
            {
                UserId = cartItem.UserId,
                ShowtimeId = cartItem.ShowtimeId, // Use null for concession-only items
                TotalAmount = cartItem.TotalAmount,
                Status = "Paid",
                PaymentMethod = "Tiền mặt",
                CreatedAt = DateTime.UtcNow,
                BookingTime = cartItem.Showtime?.ShowDateTime ?? DateTime.Now // Handle null showtime
            };_context.Bookings.Add(booking);
            await _context.SaveChangesAsync();            // Create booking seats only for ticket items (not concession-only)
            bool hasSeatBooking = cartItem.ShowtimeId != null && !string.IsNullOrEmpty(cartItem.SelectedSeats);
            
            if (hasSeatBooking)
            {
                // Use unique variable names
                var bookingSeatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var bookingSeats = await _context.Seats
                    .Where(s => bookingSeatIds.Contains(s.Id))
                    .ToListAsync();

                // Create booking seats
                foreach (var seat in bookingSeats)
                {
                    var bookingSeat = new BookingSeat
                    {
                        BookingId = booking.Id,
                        SeatId = seat.Id,
                        Price = seat.Price,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.BookingSeats.Add(bookingSeat);
                }
            }            // Update cart item status to Completed
            cartItem.Status = "Completed";
            cartItem.PaymentMethod = "Tiền mặt";
            await _context.SaveChangesAsync();            // Redirect to success page with appropriate message
            if (!hasSeatBooking)
            {
                TempData["Success"] = "Đặt hàng thành công! Vui lòng đến quầy để thanh toán và nhận đồ ăn, nước uống.";
            }
            else
            {
                TempData["Success"] = "Đặt vé thành công! Vui lòng đến quầy để thanh toán và nhận vé.";
            }
            return RedirectToAction("Details", new { id = booking.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cash payment");
            throw;
        }
    }

    private async Task<IActionResult> ProcessMomoPayment(CartItem cartItem)
    {
        try
        {
            // Check if this is a concession-only item
            bool isConcessionOnly = cartItem.ShowtimeId == null || string.IsNullOrEmpty(cartItem.SelectedSeats);

            if (!isConcessionOnly)
            {
                // Process seat booking for ticket items
                var selectedSeatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var selectedSeats = await _context.Seats
                    .Where(s => selectedSeatIds.Contains(s.Id))
                    .ToListAsync();

                if (selectedSeats.Count != selectedSeatIds.Count)
                {
                    TempData["Error"] = "Không tìm thấy thông tin ghế.";
                    return RedirectToAction("Index", "Cart");
                }

                // Đánh dấu ghế đang xử lý thanh toán
                foreach (var seat in selectedSeats)
                {
                    seat.Status = "Processing";
                    seat.IsAvailable = false; // Đánh dấu là không khả dụng để tránh đặt lại
                }
            }

            // Update cart item status
            cartItem.Status = "Processing";
            cartItem.PaymentMethod = "momo";
            await _context.SaveChangesAsync();
            
            // TODO: Implement Momo payment integration
            // For now, just show a message
            TempData["Info"] = "Chức năng thanh toán qua Momo đang được phát triển";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Momo payment");
            TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
    }

    private async Task<IActionResult> ProcessVNPayPayment(CartItem cartItem)
    {
        try
        {
            // Check if this is a concession-only item
            bool isConcessionOnly = cartItem.ShowtimeId == null || string.IsNullOrEmpty(cartItem.SelectedSeats);

            if (!isConcessionOnly)
            {
                // Process seat booking for ticket items
                var selectedSeatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var selectedSeats = await _context.Seats
                    .Where(s => selectedSeatIds.Contains(s.Id))
                    .ToListAsync();

                if (selectedSeats.Count != selectedSeatIds.Count)
                {
                    TempData["Error"] = "Không tìm thấy thông tin ghế.";
                    return RedirectToAction("Index", "Cart");
                }

                // Đánh dấu ghế đang xử lý thanh toán
                foreach (var seat in selectedSeats)
                {
                    seat.Status = "Processing";
                    seat.IsAvailable = false; // Đánh dấu là không khả dụng để tránh đặt lại
                }
            }

            // Update cart item status
            cartItem.Status = "Processing";
            cartItem.PaymentMethod = "vnpay";
            await _context.SaveChangesAsync();
            
            // TODO: Implement VNPay payment integration
            // For now, just show a message
            TempData["Info"] = "Chức năng thanh toán qua VNPay đang được phát triển";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay payment");
            TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
    }

    private async Task<IActionResult> ProcessCardPayment(CartItem cartItem)
    {
        try
        {
            // Check if this is a concession-only item
            bool isConcessionOnly = cartItem.ShowtimeId == null || string.IsNullOrEmpty(cartItem.SelectedSeats);

            if (!isConcessionOnly)
            {
                // Process seat booking for ticket items
                var selectedSeatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                var selectedSeats = await _context.Seats
                    .Where(s => selectedSeatIds.Contains(s.Id))
                    .ToListAsync();

                if (selectedSeats.Count != selectedSeatIds.Count)
                {
                    TempData["Error"] = "Không tìm thấy thông tin ghế.";
                    return RedirectToAction("Index", "Cart");
                }

                // Đánh dấu ghế đang xử lý thanh toán
                foreach (var seat in selectedSeats)
                {
                    seat.Status = "Processing";
                    seat.IsAvailable = false; // Đánh dấu là không khả dụng để tránh đặt lại
                }
            }

            // Update cart item status
            cartItem.Status = "Processing";
            cartItem.PaymentMethod = "card";
            await _context.SaveChangesAsync();
            
            // TODO: Implement card payment integration
            // For now, just show a message
            TempData["Info"] = "Chức năng thanh toán qua thẻ đang được phát triển";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing card payment");
            TempData["Error"] = "Có lỗi xảy ra khi xử lý thanh toán";
            return RedirectToAction("PaymentConfirm", new { cartItemId = cartItem.Id });
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        _logger.LogInformation($"Attempting to view booking details for ID: {id}");

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User not authenticated");
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        _logger.LogInformation($"User ID: {userId}");

        // Trước tiên thử tìm booking với ID
        var booking = await _context.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Theater)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        // Nếu không tìm thấy booking, thử tìm cartItem với ID đó
        if (booking == null)
        {
            _logger.LogInformation($"Booking not found, trying to find CartItem with ID: {id}");
              var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Theater)
                .Include(ci => ci.Cart)
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

            if (cartItem == null)
            {
                _logger.LogWarning($"Neither Booking nor CartItem with ID {id} found for user {userId}");
                return NotFound();
            }            // Nếu tìm thấy cartItem, tạo một Booking tạm thời để hiển thị
            booking = new Booking
            {
                Id = cartItem.Id,
                UserId = userId,
                ShowtimeId = cartItem.ShowtimeId ?? 0, // Add null-coalescing operator
                Showtime = cartItem.Showtime,
                TotalAmount = cartItem.TotalAmount,
                Status = cartItem.Status,
                PaymentMethod = cartItem.PaymentMethod ?? "Chưa thanh toán",
                CreatedAt = cartItem.CreatedAt,
                BookingTime = cartItem.Showtime?.ShowDateTime ?? DateTime.Now, // Handle null showtime
                BookingSeats = new List<BookingSeat>()
            };

            // Tạo BookingSeats từ thông tin ghế đã chọn
            var seatIds = cartItem.SelectedSeats?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();

            foreach (var seat in seats)
            {
                booking.BookingSeats.Add(new BookingSeat
                {
                    BookingId = booking.Id,
                    SeatId = seat.Id,
                    Seat = seat,
                    Price = seat.Price
                });
            }            _logger.LogInformation($"Successfully created temporary booking from CartItem with ID: {id}");
            
            // Pass concession items to view
            ViewBag.ConcessionItems = cartItem.ConcessionItems.ToList();
            ViewBag.TheaterName = cartItem.Showtime?.Theater?.Name ?? "";
            ViewBag.SeatTotal = booking.BookingSeats.Sum(bs => bs.Price);
        }
        else
        {
            _logger.LogInformation($"Successfully retrieved booking details for ID: {id}");
            
            // For completed bookings, try to find related concession items via CartItem
            var relatedCartItem = await _context.CartItems
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && 
                                          ci.ShowtimeId == booking.ShowtimeId && 
                                          ci.SelectedSeats == booking.SelectedSeats);
            
            ViewBag.ConcessionItems = relatedCartItem?.ConcessionItems?.ToList() ?? new List<ConcessionItem>();
            ViewBag.TheaterName = booking.Showtime?.Theater?.Name ?? "";
            ViewBag.SeatTotal = booking.BookingSeats.Sum(bs => bs.Price);
        }

        return View(booking);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPayment(int cartItemId, string customerId, string returnUrl)
    {
        try
        {            // Kiểm tra cartItem và customer
            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(ci => ci.Cart)
                .Include(ci => ci.ConcessionItems)
                    .ThenInclude(ci => ci.Concession)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == customerId);

            if (cartItem == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt vé.";
                return Redirect(returnUrl ?? "/");
            }

            // Kiểm tra trạng thái
            if (cartItem.Status != "Pending")
            {
                TempData["Error"] = "Đơn hàng này đã được xử lý.";
                return Redirect(returnUrl ?? "/");
            }

            // Load seat information
            var seatIds = cartItem.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();

            if (!seats.Any())
            {
                TempData["Error"] = "Không tìm thấy thông tin ghế.";
                return Redirect(returnUrl ?? "/");
            }            // Tạo booking mới
            var booking = new Booking
            {
                UserId = customerId,
                ShowtimeId = cartItem.ShowtimeId ?? 0, // Add null-coalescing operator
                TotalAmount = cartItem.TotalAmount,
                Status = "Paid",
                PaymentMethod = "Admin",
                CreatedAt = DateTime.UtcNow,
                BookingTime = cartItem.Showtime?.ShowDateTime ?? DateTime.Now, // Handle null showtime
                BookingDate = DateTime.Now
            };

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    // Cập nhật trạng thái ghế
                    foreach (var seat in seats)
                    {
                        seat.IsAvailable = false;
                        seat.Status = "Booked";

                        var bookingSeat = new BookingSeat
                        {
                            BookingId = booking.Id,
                            SeatId = seat.Id,
                            Price = seat.Price,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.BookingSeats.Add(bookingSeat);
                    }

                    // Cập nhật trạng thái cart item
                    cartItem.Status = "Completed";
                    cartItem.PaymentMethod = "Admin";

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Thanh toán vé thành công!";
                    return Redirect(returnUrl ?? "/");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            return Redirect(returnUrl ?? "/");
        }
    }

    // Method to handle payment of all pending items in the cart
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> PaymentConfirmAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
          // Get all pending items in the cart
        var pendingItems = await _context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.ConcessionItems)
                .ThenInclude(ci => ci.Concession)
            .Where(ci => ci.UserId == userId && ci.Status == "Pending")
            .ToListAsync();
            
        if (pendingItems == null || !pendingItems.Any())
        {
            TempData["Error"] = "Không có sản phẩm nào cần thanh toán";
            return RedirectToAction("Index", "Cart");
        }
        
        // Calculate total amount for all pending items
        var totalAmount = pendingItems.Sum(ci => ci.TotalAmount);
        
        // For simplicity, we'll just mark all items as completed
        foreach (var item in pendingItems)
        {
            item.Status = "Completed";
            item.PaymentMethod = "cash"; // Default to cash payment
        }
        
        await _context.SaveChangesAsync();
        
        TempData["Success"] = $"Thanh toán thành công {pendingItems.Count} sản phẩm với tổng số tiền {totalAmount:N0} đ";
        return RedirectToAction("Index", "Cart");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadTicketPdf(int id)
    {
        // Lấy thông tin booking
        var booking = await _context.Bookings
            .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
            .Include(b => b.Showtime).ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null)
        {
            // Nếu không tìm thấy booking, thử tìm cartItem với ID đó
            var cartItem = await _context.CartItems
                .Include(ci => ci.Showtime).ThenInclude(s => s.Movie)
                .Include(ci => ci.Showtime).ThenInclude(s => s.Theater)
                .FirstOrDefaultAsync(ci => ci.Id == id);
            if (cartItem == null)
            {
                return NotFound("Không tìm thấy vé với mã này.");
            }
            // Kiểm tra quyền: chỉ cho phép user đã đặt vé hoặc admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && cartItem.UserId != userId && cartItem.Cart.UserId != userId)
            {
                return Unauthorized();
            }
            // Lấy thông tin ghế
            var seatIds = cartItem.SelectedSeats?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() ?? new List<int>();
            var seats = await _context.Seats.Where(s => seatIds.Contains(s.Id)).ToListAsync();
            // Tạo mã QR từ mã cartItem
            string qrContentCart = $"CartItem:{cartItem.Id}";
            using var qrGeneratorCart = new QRCoder.QRCodeGenerator();
            using var qrDataCart = qrGeneratorCart.CreateQrCode(qrContentCart, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrPngCart = new QRCoder.PngByteQRCode(qrDataCart);
            byte[] qrBytesCart = qrPngCart.GetGraphic(20);
            // Lấy thông tin bắp nước
            var concessionItems = await _context.ConcessionItems
                .Include(ci => ci.Concession)
                .Where(ci => ci.CartItemId == cartItem.Id)
                .ToListAsync();
            // Tạo PDF
            var documentCart = new PdfSharpCore.Pdf.PdfDocument();
            var pageCart = documentCart.AddPage();
            pageCart.Width = 400;
            pageCart.Height = 600;
            var gfxCart = PdfSharpCore.Drawing.XGraphics.FromPdfPage(pageCart);
            var fontTitleCart = new PdfSharpCore.Drawing.XFont("Arial", 18, PdfSharpCore.Drawing.XFontStyle.Bold);
            var fontNormalCart = new PdfSharpCore.Drawing.XFont("Arial", 12, PdfSharpCore.Drawing.XFontStyle.Regular);
            var fontSmallCart = new PdfSharpCore.Drawing.XFont("Arial", 10, PdfSharpCore.Drawing.XFontStyle.Regular);
            int yCart = 40;
            gfxCart.DrawString("Vé Xem Phim", fontTitleCart, PdfSharpCore.Drawing.XBrushes.DarkBlue, new PdfSharpCore.Drawing.XRect(0, yCart, pageCart.Width, 30), PdfSharpCore.Drawing.XStringFormats.TopCenter);
            yCart += 40;
            gfxCart.DrawString($"Mã vé tạm: {cartItem.Id}", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 25;
            gfxCart.DrawString($"Trạng thái: {cartItem.Status}", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 25;
            gfxCart.DrawString($"Phim: {cartItem.Showtime?.Movie?.Title ?? ""}", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 20;
            gfxCart.DrawString($"Suất chiếu: {cartItem.Showtime?.ShowDateTime:dd/MM/yyyy HH:mm}", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 20;
            gfxCart.DrawString($"Ghế: {string.Join(", ", seats.Select(s => s.Row + s.Number.ToString()))}", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 20;
            gfxCart.DrawString($"Tổng tiền ghế: {seats.Sum(s => s.Price):N0} VNĐ", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 20;
            // Bắp nước
            if (concessionItems.Any())
            {
                gfxCart.DrawString("Bắp nước & Đồ ăn:", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
                yCart += 18;
                foreach (var item in concessionItems)
                {
                    string line = $"- {item.Concession.Name} x{item.Quantity}";
                    if (item.Discount > 0)
                        line += $" (Giảm: {item.Discount:N0}đ)";
                    line += $" - {(item.Price * item.Quantity - item.Discount):N0}đ";
                    gfxCart.DrawString(line, fontSmallCart, PdfSharpCore.Drawing.XBrushes.Black, 50, yCart);
                    yCart += 15;
                }
            }
            yCart += 10;
            gfxCart.DrawString($"Tổng tiền: {cartItem.TotalAmount:N0} VNĐ", fontNormalCart, PdfSharpCore.Drawing.XBrushes.Black, 40, yCart);
            yCart += 30;
            // Vẽ mã QR
            using (var msQrCart = new System.IO.MemoryStream(qrBytesCart))
            using (var imgCart = PdfSharpCore.Drawing.XImage.FromStream(() => msQrCart))
            {
                gfxCart.DrawImage(imgCart, (pageCart.Width - 150) / 2, yCart, 150, 150);
            }
            yCart += 170;
            gfxCart.DrawString("Quét mã QR để check-in tại rạp (khi đã thanh toán)", fontSmallCart, PdfSharpCore.Drawing.XBrushes.DarkGray, new PdfSharpCore.Drawing.XRect(0, yCart, pageCart.Width, 20), PdfSharpCore.Drawing.XStringFormats.TopCenter);
            // Xuất PDF ra stream
            using var streamCart = new System.IO.MemoryStream();
            documentCart.Save(streamCart, false);
            streamCart.Position = 0;
            return File(streamCart.ToArray(), "application/pdf", $"VeTam_{cartItem.Id}.pdf");
        }
        // Kiểm tra quyền: chỉ cho phép user đã đặt vé hoặc admin
        var userIdBooking = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdminBooking = User.IsInRole("Admin");
        if (!isAdminBooking && booking.UserId != userIdBooking)
        {
            return Unauthorized();
        }
        // Tạo mã QR từ mã booking (dùng đúng QRCoder 1.6.0)
        string qrContent = $"Booking:{booking.Id}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        var qrPng = new PngByteQRCode(qrData);
        byte[] qrBytes = qrPng.GetGraphic(20);

        // Tạo PDF
        var document = new PdfDocument();
        var page = document.AddPage();
        page.Width = 400;
        page.Height = 600;
        var gfx = XGraphics.FromPdfPage(page);
        var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
        var fontNormal = new XFont("Arial", 12, XFontStyle.Regular);
        int y = 40;
        gfx.DrawString("Vé Xem Phim", fontTitle, XBrushes.DarkBlue, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
        y += 40;
        gfx.DrawString($"Mã vé: {booking.Id}", fontNormal, XBrushes.Black, 40, y);
        y += 30;
        gfx.DrawString($"Phim: {booking.Showtime.Movie.Title}", fontNormal, XBrushes.Black, 40, y);
        y += 25;
        gfx.DrawString($"Suất chiếu: {booking.Showtime.ShowDateTime:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Black, 40, y);
        y += 25;
        gfx.DrawString($"Ghế: {string.Join(", ", booking.BookingSeats.Select(bs => bs.Seat.Row + bs.Seat.Number.ToString()))}", fontNormal, XBrushes.Black, 40, y);
        y += 25;
        gfx.DrawString($"Tổng tiền: {booking.TotalAmount:N0} VNĐ", fontNormal, XBrushes.Black, 40, y);
        y += 40;
        // Vẽ mã QR
        using (var msQr = new MemoryStream(qrBytes))
        using (var img = XImage.FromStream(() => msQr))
        {
            gfx.DrawImage(img, (page.Width - 150) / 2, y, 150, 150);
        }
        y += 170;
        gfx.DrawString("Quét mã QR để check-in tại rạp", fontNormal, XBrushes.DarkGray, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
        // Xuất PDF ra stream
        using var stream = new MemoryStream();
        document.Save(stream, false);
        stream.Position = 0;
        return File(stream.ToArray(), "application/pdf", $"Ve_{booking.Id}.pdf");
    }

    [HttpGet]
    public IActionResult QrCode(int bookingId)
    {
        string qrContent = $"Booking:{bookingId}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        var qrBmp = new BitmapByteQRCode(qrData);
        byte[] qrBytes = qrBmp.GetGraphic(20);
        return File(qrBytes, "image/png");
    }

    [HttpGet]
    public IActionResult PaymentQr(string method, int cartItemId)
    {
        // Tạo nội dung QR demo (thực tế sẽ lấy từ API MoMo/VNPay)
        string qrContent = method.ToLower() switch
        {
            "momo" => $"https://momo.vn/pay?order={cartItemId}",
            "vnpay" => $"https://vnpay.vn/pay?order={cartItemId}",
            _ => "https://yourdomain.com"
        };
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(qrContent, QRCoder.QRCodeGenerator.ECCLevel.Q);
        var qrBmp = new QRCoder.BitmapByteQRCode(qrData);
        byte[] qrBytes = qrBmp.GetGraphic(20);
        return File(qrBytes, "image/png");
    }

    // AJAX: Lấy danh sách rạp có suất chiếu cho phim
    [HttpGet]
    public async Task<IActionResult> GetTheatersByMovie(int movieId)
    {
        var theaters = await _context.Showtimes
            .Where(s => s.MovieId == movieId)
            .Select(s => s.Theater)
            .Distinct()
            .ToListAsync();
        ViewBag.MovieId = movieId; // Đảm bảo truyền MovieId sang PartialView
        return PartialView("_TheaterAccordion", theaters);
    }

    // AJAX: Lấy danh sách suất chiếu của phim tại một rạp
    [HttpGet]
    public async Task<IActionResult> GetShowtimesByTheater(int movieId, int theaterId)
    {
        var showtimes = await _context.Showtimes
            .Where(s => s.MovieId == movieId && s.TheaterId == theaterId)
            .OrderBy(s => s.ShowDateTime)
            .ToListAsync();
        return PartialView("_ShowtimeList", showtimes);
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
            _logger.LogError(ex, $"Error resetting seat status for cart item {cartItemId}");
        }
    }

    /// <summary>
    /// Cancel payment and release seats (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
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
            return RedirectToAction("Index", "Ticket", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling payment for cart item {cartItemId}");
            TempData["Error"] = "Có lỗi xảy ra khi hủy thanh toán";
            return RedirectToAction("Index", "Ticket", new { area = "Admin" });
        }
    }

    /// <summary>
    /// Auto-release seats that have been in processing status for too long (Admin only)
    /// </summary>
    [HttpGet, HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoReleaseSeats()
    {
        try
        {
            // Find cart items that have been in processing status for more than 1 phút (phút, theo yêu cầu UI)
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
            return RedirectToAction("Index", "Ticket", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-releasing seats");
            TempData["Error"] = "Có lỗi xảy ra khi tự động giải phóng ghế";
            return RedirectToAction("Index", "Ticket", new { area = "Admin" });
        }
    }
}

