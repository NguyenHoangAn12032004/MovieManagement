using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Areas.Admin.Models;
using MovieManagement.Data;
using MovieManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MovieManagement.Hubs;
using Microsoft.Extensions.Logging;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SeatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SeatHub> _hubContext;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(ApplicationDbContext context, IHubContext<SeatHub> hubContext, ILogger<SeatsController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: Admin/Seats
        public async Task<IActionResult> Index()
        {
            // Get all theaters
            var theaters = await _context.Theaters.ToListAsync();
            
            // Get all seats with their relationships
            var seats = await _context.Seats
                .Include(s => s.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(s => s.Showtime.Theater)
                .ToListAsync();
            
            // Create the admin view model with statistics
            var viewModel = new AdminSeatViewModel
            {
                Seats = seats,
                Theaters = theaters,
                TotalSeats = seats.Count,
                AvailableSeats = seats.Count(s => s.IsAvailable && s.Status == "Available"),
                MaintenanceSeats = seats.Count(s => s.Status == "Maintenance"),
                CleaningSeats = seats.Count(s => s.Status == "Cleaning"),
                BookedSeats = seats.Count(s => !s.IsAvailable)
            };
            
            return View(viewModel);
        }

        // GET: Admin/Seats/Theater/5
        public async Task<IActionResult> Theater(int id)
        {
            var theater = await _context.Theaters.FindAsync(id);
            
            if (theater == null)
            {
                return NotFound();
            }
            
            // Get all seats for this theater
            var seats = await _context.Seats
                .Include(s => s.Showtime)
                .Where(s => s.Showtime.TheaterId == id)
                .ToListAsync();
                
            ViewBag.Theater = theater;
            
            return View(seats);
        }

        // GET: Admin/Seats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(s => s.Showtime.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (seat == null)
            {
                return NotFound();
            }

            // Get booking history for this seat
            var bookingHistory = await _context.BookingSeats
                .Where(bs => bs.SeatId == id)
                .Include(bs => bs.Booking)
                .ThenInclude(b => b.User)
                .Include(bs => bs.Booking.Showtime)
                .ThenInclude(s => s.Movie)
                .OrderByDescending(bs => bs.Booking.BookingTime)
                .Take(5)
                .Select(bs => new BookingHistory
                {
                    BookingId = bs.BookingId,
                    UserName = bs.Booking.User.UserName,
                    BookingDate = bs.Booking.BookingTime,
                    MovieTitle = bs.Booking.Showtime.Movie.Title,
                    ShowtimeDateTime = bs.Booking.Showtime.ShowDateTime
                })
                .ToListAsync();

            var viewModel = new SeatDetailsViewModel
            {
                Seat = seat,
                Theater = seat.Showtime.Theater,
                BookingHistory = bookingHistory
            };

            return View(viewModel);
        }

        // POST: Admin/Seats/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            _logger.LogInformation($"UpdateStatus called with id={id}, status={status}");
            try
            {
                var seat = await _context.Seats.FindAsync(id);
                if (seat == null)
                {
                    _logger.LogWarning($"Seat with id={id} not found");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = $"Ghế với id={id} không tìm thấy. Có thể bạn cần tải lại trang để cập nhật dữ liệu mới nhất." });
                    }
                    return NotFound($"Ghế với id={id} không tìm thấy. Có thể bạn cần tải lại trang để cập nhật dữ liệu mới nhất.");
                }
                // Chỉ cập nhật ghế hiện tại
                seat.Status = status;
                seat.IsAvailable = status == "Available";
                _context.Seats.Update(seat);
                await _context.SaveChangesAsync();
                // Gửi thông báo SignalR cho client
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveSeatUpdate",
                    seat.Id,
                    seat.Row,
                    seat.Number,
                    seat.Status,
                    seat.IsAvailable);
                _logger.LogInformation($"Successfully updated seat {seat.Id} (ShowtimeId {seat.ShowtimeId})");
                return Json(new {
                    success = true,
                    message = "Cập nhật trạng thái ghế thành công",
                    updatedCount = 1,
                    showtimeId = seat.ShowtimeId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating seat: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi khi cập nhật trạng thái ghế: {ex.Message}" });
            }
        }
        
        // GET: Admin/Seats/ManageByShowtime
        public async Task<IActionResult> ManageByShowtime()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .OrderByDescending(s => s.ShowDateTime)
                .ToListAsync();
                
            return View(showtimes);
        }
        
        // GET: Admin/Seats/ShowtimeSeats/5
        public async Task<IActionResult> ShowtimeSeats(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (showtime == null)
            {
                return NotFound();
            }
            
            var seats = await _context.Seats
                .Where(s => s.ShowtimeId == id)
                .ToListAsync();
                
            var viewModel = new SeatViewModel
            {
                ShowtimeId = showtime.Id,
                MovieTitle = showtime.Movie.Title,
                ShowtimeDateTime = showtime.ShowDateTime,
                Seats = seats
            };
            
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetShowtimesByTheater(int theaterId)
        {
            var showtimes = await _context.Showtimes
                .Where(s => s.TheaterId == theaterId)
                .OrderBy(s => s.ShowDateTime)
                .Select(s => new {
                    id = s.Id,
                    movieTitle = s.Movie.Title,
                    showDateTime = s.ShowDateTime.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();
            return Json(showtimes);
        }

        [HttpGet]
        public async Task<IActionResult> GetSeatsByShowtime(int showtimeId)
        {
            var seats = await _context.Seats
                .Where(s => s.ShowtimeId == showtimeId)
                .Select(s => new {
                    id = s.Id,
                    row = s.Row,
                    number = s.Number,
                    type = s.Type,
                    isAvailable = s.IsAvailable,
                    status = s.Status,
                    price = s.Price
                })
                .OrderBy(s => s.row).ThenBy(s => s.number)
                .ToListAsync();
            return Json(seats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedSeatsForAllShowtimes()
        {
            var allShowtimes = await _context.Showtimes.ToListAsync();
            int createdCount = 0;
            foreach (var showtime in allShowtimes)
            {
                bool hasSeats = await _context.Seats.AnyAsync(s => s.ShowtimeId == showtime.Id);
                if (hasSeats) continue;
                // Lấy ghế mẫu từ lịch chiếu gần nhất cùng rạp (nếu có)
                var lastShowtime = await _context.Showtimes
                    .Where(s => s.TheaterId == showtime.TheaterId && s.Id != showtime.Id)
                    .OrderByDescending(s => s.ShowDateTime)
                    .FirstOrDefaultAsync();
                List<Seat> seatTemplates = new List<Seat>();
                if (lastShowtime != null)
                {
                    seatTemplates = await _context.Seats
                        .Where(s => s.ShowtimeId == lastShowtime.Id)
                        .ToListAsync();
                }
                if (seatTemplates.Count == 0)
                {
                    for (char row = 'A'; row <= 'J'; row++)
                    {
                        for (int num = 1; num <= 12; num++)
                        {
                            seatTemplates.Add(new Seat
                            {
                                Row = row.ToString(),
                                Number = num,
                                Type = "Standard",
                                IsAvailable = true,
                                Status = "Available",
                                Price = showtime.Price
                            });
                        }
                    }
                }
                var newSeats = seatTemplates.Select(s => new Seat
                {
                    Row = s.Row,
                    Number = s.Number,
                    Type = s.Type,
                    IsAvailable = true,
                    Status = "Available",
                    Price = s.Price,
                    ShowtimeId = showtime.Id
                }).ToList();
                _context.Seats.AddRange(newSeats);
                createdCount += newSeats.Count;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã tạo {createdCount} ghế cho các lịch chiếu chưa có ghế.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAllSeats()
        {
            try
            {
                // Xóa tất cả ghế hiện có
                var allSeats = await _context.Seats.ToListAsync();
                _context.Seats.RemoveRange(allSeats);
                await _context.SaveChangesAsync();

                // Lấy tất cả showtime
                var allShowtimes = await _context.Showtimes.ToListAsync();
                int createdCount = 0;

                foreach (var showtime in allShowtimes)
                {
                    // Tạo ghế mới cho mỗi showtime
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
                                ShowtimeId = showtime.Id
                            });
                        }
                    }

                    _context.Seats.AddRange(newSeats);
                    createdCount += newSeats.Count;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã reset và tạo lại {createdCount} ghế cho {allShowtimes.Count} lịch chiếu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting seats: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi reset ghế: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
} 