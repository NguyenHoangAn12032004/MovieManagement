using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Staff = MovieManagement.Models.Staff;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Staff
        public async Task<IActionResult> Index()
        {
            return View(await _context.Staffs.ToListAsync());
        }

        // GET: Admin/Staff/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Admin/Staff/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Admin/Staff/CreateWithAccount
        public IActionResult CreateWithAccount()
        {
            return View(new StaffWithAccountViewModel());
        }

        // POST: Admin/Staff/CreateWithAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithAccount(StaffWithAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate account creation fields if CreateAccount is true
                if (model.CreateAccount)
                {
                    if (string.IsNullOrWhiteSpace(model.UserName))
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập là bắt buộc khi tạo tài khoản");
                    }
                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        ModelState.AddModelError("Password", "Mật khẩu là bắt buộc khi tạo tài khoản");
                    }
                    if (string.IsNullOrWhiteSpace(model.FirstName))
                    {
                        ModelState.AddModelError("FirstName", "Họ là bắt buộc khi tạo tài khoản");
                    }
                    if (string.IsNullOrWhiteSpace(model.LastName))
                    {
                        ModelState.AddModelError("LastName", "Tên là bắt buộc khi tạo tài khoản");
                    }

                    // Check if username already exists
                    var existingUser = await _userManager.FindByNameAsync(model.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại");
                    }

                    // Check if email already exists
                    var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingEmailUser != null)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng cho tài khoản khác");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    // Create Staff record
                    var staff = new Staff
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        HourlyRate = model.HourlyRate,
                        Bonus = model.Bonus,
                        Penalty = model.Penalty,
                        Position = model.Position,
                        Gender = model.Gender,
                        BirthDate = model.BirthDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Add(staff);
                    await _context.SaveChangesAsync();

                    // Create ApplicationUser if requested
                    if (model.CreateAccount)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = model.UserName,
                            Email = model.Email,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            EmailConfirmed = true
                        };

                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            // Check if Staff role exists, if not create it
                            if (!await _context.Roles.AnyAsync(r => r.Name == "Staff"))
                            {
                                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                                await roleManager.CreateAsync(new IdentityRole("Staff"));
                            }
                            
                            // Add Staff role to the user
                            await _userManager.AddToRoleAsync(user, "Staff");
                            TempData["SuccessMessage"] = $"Nhân viên '{staff.FullName}' và tài khoản đăng nhập đã được tạo thành công!";
                        }
                        else
                        {
                            // If user creation failed, we should handle this
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            TempData["ErrorMessage"] = $"Nhân viên đã được tạo nhưng tài khoản đăng nhập thất bại: {errors}";
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Nhân viên '{staff.FullName}' đã được tạo thành công!";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    return View(model);
                }
            }
            return View(model);
        }

        // POST: Admin/Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Email,PhoneNumber,WorkHours,HourlyRate,Bonus,Penalty,Position,Gender,BirthDate")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                staff.CreatedAt = DateTime.Now;
                staff.UpdatedAt = DateTime.Now;
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Admin/Staff/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        // POST: Admin/Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,WorkHours,HourlyRate,Bonus,Penalty,Position,Gender,BirthDate,CreatedAt")] Staff staff)
        {
            if (id != staff.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    staff.UpdatedAt = DateTime.Now;
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.Id))
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
            return View(staff);
        }

        // GET: Admin/Staff/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // POST: Admin/Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Staff/Schedule/5
        public async Task<IActionResult> Schedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                return NotFound();
            }

            // Lấy ngày đầu tuần (thứ 2)
            DateTime today = DateTime.Now;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = today.AddDays(-1 * diff);
            ViewBag.WeekRange = $"{monday.ToString("dd/MM")} - {monday.AddDays(6).ToString("dd/MM/yyyy")}";
            ViewBag.Staff = staff;

            return View(staff);
        }

        // POST: Admin/Staff/SaveSchedule
        [HttpPost]
        public async Task<IActionResult> SaveSchedule([FromBody] List<StaffScheduleViewModel> schedules)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (schedules == null || schedules.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Không có dữ liệu lịch làm để lưu" });
                }

                // Lấy ngày hiện tại (không tính giờ phút giây)
                DateTime today = DateTime.Today;

                // Xóa schedule cũ nếu có
                var staffId = schedules[0].StaffId;
                var existingSchedules = await _context.StaffSchedules
                    .Where(s => s.StaffId == staffId)
                    .ToListAsync();
                
                if (existingSchedules.Any())
                {
                    _context.StaffSchedules.RemoveRange(existingSchedules);
                    await _context.SaveChangesAsync();
                }

                // Lấy thông tin nhân viên để cập nhật số giờ làm
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                // Đếm tổng số giờ theo ca
                decimal totalHours = 0;

                // Thêm các schedule mới
                foreach (var schedule in schedules)
                {
                    // Parse ngày từ định dạng YYYY-MM-DD
                    DateTime scheduleDate;
                    if (!DateTime.TryParse(schedule.Date, out scheduleDate))
                    {
                        return BadRequest(new { success = false, message = $"Định dạng ngày không hợp lệ: {schedule.Date}" });
                    }

                    // Kiểm tra ngày quá khứ
                    if (scheduleDate.Date < today)
                    {
                        return BadRequest(new { success = false, message = "Không thể đăng ký lịch làm cho ngày quá khứ" });
                    }

                    // Tính số giờ làm theo ca
                    decimal hoursForShift = 0;
                    switch (schedule.Shift)
                    {
                        case "Ca1": // 6:00-11:30
                            hoursForShift = 5.5m;
                            break;
                        case "Ca2": // 12:30-18:00
                            hoursForShift = 5.5m;
                            break;
                        case "Ca3": // 19:00-2:00
                            hoursForShift = 7m;
                            break;
                    }

                    // Cộng vào tổng số giờ
                    totalHours += hoursForShift;

                    var newSchedule = new StaffSchedule
                    {
                        StaffId = staffId,
                        Date = scheduleDate,
                        Shift = schedule.Shift,
                        IsApproved = false,
                        Note = $"Đăng ký mới - {hoursForShift} giờ"
                    };

                    _context.StaffSchedules.Add(newSchedule);
                }

                // Cập nhật số giờ làm của nhân viên
                // staff.WorkHours += totalHours;
                staff.UpdatedAt = DateTime.Now;
                _context.Update(staff);

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Lịch làm đã được lưu thành công!", hoursAdded = totalHours });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lưu lịch làm: " + ex.Message });
            }
        }

        // View model để nhận dữ liệu từ client
        public class StaffScheduleViewModel
        {
            public int StaffId { get; set; }
            public string Date { get; set; }
            public string Shift { get; set; }
        }

        // GET: Admin/Staff/GetStaffSchedule
        [HttpGet]
        public async Task<IActionResult> GetStaffSchedule(int staffId, DateTime startDate)
        {
            try
            {
                // Lấy ngày kết thúc (chủ nhật)
                DateTime endDate = startDate.AddDays(6);

                // Lấy lịch làm việc trong khoảng thời gian
                var schedules = await _context.StaffSchedules
                    .Where(s => s.StaffId == staffId && s.Date >= startDate && s.Date <= endDate)
                    .Select(s => new
                    {
                        s.Date,
                        s.Shift,
                        s.IsApproved,
                        s.Note
                    })
                    .ToListAsync();

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch làm: " + ex.Message });
            }
        }

        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.Id == id);
        }
    }
} 