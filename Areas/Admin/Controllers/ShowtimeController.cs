using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShowtimeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Showtime
        public async Task<IActionResult> Index(DateTime? searchDate)
        {
            IQueryable<Showtime> query = _context.Showtimes;

            if (searchDate.HasValue)
            {
                DateTime startDate = searchDate.Value.Date;
                DateTime endDate = startDate.AddDays(1).AddTicks(-1);
                
                query = query.Where(s => s.ShowDateTime >= startDate && s.ShowDateTime <= endDate);
                ViewData["CurrentDate"] = searchDate.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                // Default to today's showtimes
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1).AddTicks(-1);
                
                query = query.Where(s => s.ShowDateTime >= today && s.ShowDateTime <= tomorrow);
                ViewData["CurrentDate"] = today.ToString("yyyy-MM-dd");
            }

            var showtimes = await query
                .Include(s => s.Movie)
                .OrderBy(s => s.ShowDateTime)
                .ToListAsync();
                
            return View(showtimes);
        }

        // POST: Admin/Showtime/ChangeAllStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAllStatus(DateTime? date)
        {
            DateTime targetDate = date?.Date ?? DateTime.Today;
            DateTime endDate = targetDate.AddDays(1).AddTicks(-1);
            
            var showtimes = await _context.Showtimes
                .Where(s => s.ShowDateTime >= targetDate && s.ShowDateTime <= endDate)
                .ToListAsync();

            // Toggle status based on current time
            // This is just an example - in real application you might want
            // to change to a specific status rather than toggling
            DateTime now = DateTime.Now;
            foreach (var showtime in showtimes)
            {
                if (showtime.ShowDateTime < now)
                {
                    // Set to active - this is just an example
                    // You might want to change this logic based on your requirements
                    showtime.ShowDateTime = now.AddHours(1);
                }
                else
                {
                    // Set to inactive
                    showtime.ShowDateTime = now.AddHours(-1);
                }
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index), new { searchDate = targetDate.ToString("yyyy-MM-dd") });
        }

        // POST: Admin/Showtime/DeleteAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(DateTime? date)
        {
            DateTime targetDate = date?.Date ?? DateTime.Today;
            DateTime endDate = targetDate.AddDays(1).AddTicks(-1);
            
            var showtimes = await _context.Showtimes
                .Where(s => s.ShowDateTime >= targetDate && s.ShowDateTime <= endDate)
                .ToListAsync();

            _context.Showtimes.RemoveRange(showtimes);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Showtime/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // GET: Admin/Showtime/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Movies, "Id", "Title");
            ViewData["TheaterId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Set<Theater>(), "Id", "Name");
            return View();
        }

        // POST: Admin/Showtime/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,TheaterId,ShowDateTime,ScreenType,Price,AvailableSeats")] Showtime showtime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(showtime);
                await _context.SaveChangesAsync();

                // Tạo ghế cho lịch chiếu mới
                // Lấy ghế mẫu từ lịch chiếu gần nhất của rạp này (nếu có)
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
                // Nếu không có lịch chiếu nào trước đó, tự định nghĩa mẫu ghế mặc định (10 hàng x 12 ghế)
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
                // Clone ghế cho lịch chiếu mới
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
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["MovieId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Movies, "Id", "Title", showtime.MovieId);
            ViewData["TheaterId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Set<Theater>(), "Id", "Name", showtime.TheaterId);
            return View(showtime);
        }

        // GET: Admin/Showtime/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }

            ViewData["MovieId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Movies, "Id", "Title", showtime.MovieId);
            ViewData["TheaterId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Set<Theater>(), "Id", "Name", showtime.TheaterId);
            return View(showtime);
        }

        // POST: Admin/Showtime/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,TheaterId,ShowDateTime,ScreenType,Price,AvailableSeats")] Showtime showtime)
        {
            if (id != showtime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showtime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(showtime.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MovieId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Movies, "Id", "Title", showtime.MovieId);
            ViewData["TheaterId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Set<Theater>(), "Id", "Name", showtime.TheaterId);
            return View(showtime);
        }

        // GET: Admin/Showtime/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: Admin/Showtime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.Id == id);
        }
    }
} 