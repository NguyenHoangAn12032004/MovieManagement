using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Staff = MovieManagement.Models.Staff;

namespace MovieManagement.Areas.StaffArea.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
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
            var user = await _userManager.GetUserAsync(User);
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);

            // Thống kê cơ bản cho nhân viên
            var todayTickets = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt.Date == DateTime.Today)
                .CountAsync();

            var todayRevenue = await _context.CartItems
                .Where(ci => ci.Status == "Completed" && ci.CreatedAt.Date == DateTime.Today)
                .SumAsync(ci => ci.TotalAmount);

            var pendingTickets = await _context.CartItems
                .Where(ci => ci.Status == "Pending" || ci.Status == "Processing")
                .CountAsync();

            ViewBag.TodayTickets = todayTickets;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.PendingTickets = pendingTickets;
            ViewBag.StaffInfo = staff;

            return View();
        }

        // Xem thông tin cá nhân
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);

            if (staff == null)
            {
                // Tạo staff record tạm thời nếu chưa có
                staff = new MovieManagement.Models.Staff
                {
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Position = "Nhân viên",
                    Gender = "Chưa xác định",
                    BirthDate = new DateTime(1990, 1, 1),
                    HourlyRate = 50000,
                    Bonus = 0,
                    Penalty = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();
            }

            return View(staff);
        }

        // Lịch làm việc
        public async Task<IActionResult> Schedule()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);

            if (staff == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction("Index");
            }

            var schedules = await _context.StaffSchedules
                .Where(ss => ss.StaffId == staff.Id)
                .OrderBy(ss => ss.Date)
                .ToListAsync();

            ViewBag.StaffInfo = staff;
            return View(schedules);
        }
    }
}
