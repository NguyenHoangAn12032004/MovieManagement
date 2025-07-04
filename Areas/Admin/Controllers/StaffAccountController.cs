using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using MovieManagement.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Staff = MovieManagement.Models.Staff;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public StaffAccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Admin/StaffAccount
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách user có role "Staff"
            var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");
            
            var staffAccounts = new List<StaffAccountViewModel>();
            
            foreach (var user in staffUsers)
            {
                var staffInfo = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);
                staffAccounts.Add(new StaffAccountViewModel
                {
                    User = user,
                    Staff = staffInfo,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return View(staffAccounts);
        }

        // GET: Admin/StaffAccount/Create
        public async Task<IActionResult> Create()
        {
            var staffsWithoutAccount = await _context.Staffs
                .Where(s => !_context.Users.Any(u => u.Email == s.Email))
                .ToListAsync();
                
            var totalStaffAccounts = await _context.Users
                .Where(u => _context.Staffs.Any(s => s.Email == u.Email))
                .CountAsync();
                
            ViewBag.Staffs = staffsWithoutAccount;
            ViewBag.TotalStaffAccounts = totalStaffAccounts;
            
            return View();
        }

        // POST: Admin/StaffAccount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var staff = await _context.Staffs.FindAsync(model.StaffId);
                if (staff == null)
                {
                    ModelState.AddModelError("", "Nhân viên không tồn tại.");
                    ViewBag.Staffs = await _context.Staffs
                        .Where(s => !_context.Users.Any(u => u.Email == s.Email))
                        .ToListAsync();
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _userManager.FindByEmailAsync(staff.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email này đã có tài khoản trong hệ thống.");
                    ViewBag.Staffs = await _context.Staffs
                        .Where(s => !_context.Users.Any(u => u.Email == s.Email))
                        .ToListAsync();
                    return View(model);
                }

                // Tách tên một cách chính xác hơn
                var nameParts = staff.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var firstName = nameParts.FirstOrDefault() ?? "";
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                var user = new ApplicationUser
                {
                    UserName = staff.Email,
                    Email = staff.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = staff.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Staff");
                    TempData["Success"] = "Tạo tài khoản nhân viên thành công.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Staffs = await _context.Staffs
                .Where(s => !_context.Users.Any(u => u.Email == s.Email))
                .ToListAsync();
            return View(model);
        }

        // GET: Admin/StaffAccount/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin nhân viên tương ứng.";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditStaffAccountViewModel
            {
                UserId = user.Id,
                StaffId = staff.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.Now,
                SelectedRoles = userRoles.ToList(),
                Staff = staff
            };

            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = allRoles;

            return View(model);
        }

        // POST: Admin/StaffAccount/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditStaffAccountViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    var staff = await _context.Staffs.FindAsync(model.StaffId);
                    if (staff == null)
                    {
                        TempData["Error"] = "Không tìm thấy thông tin nhân viên.";
                        return View(model);
                    }

                    // Cập nhật email nếu thay đổi
                    if (user.Email != model.Email)
                    {
                        // Kiểm tra email trùng lặp
                        var existingUser = await _userManager.FindByEmailAsync(model.Email);
                        if (existingUser != null && existingUser.Id != user.Id)
                        {
                            ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
                            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                            model.Staff = staff;
                            return View(model);
                        }

                        user.Email = model.Email;
                        user.NormalizedEmail = model.Email.ToUpper();
                        
                        // Cập nhật email trong bảng Staff
                        staff.Email = model.Email;
                        _context.Staffs.Update(staff);
                    }

                    // Cập nhật trạng thái tài khoản
                    if (model.IsActive)
                    {
                        user.LockoutEnd = null;
                    }
                    else
                    {
                        user.LockoutEnd = DateTimeOffset.MaxValue;
                    }

                    // Cập nhật mật khẩu nếu có
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
                            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                            model.Staff = staff;
                            return View(model);
                        }
                    }

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        // Cập nhật roles
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>());
                        var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles);

                        if (rolesToRemove.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        }

                        if (rolesToAdd.Any())
                        {
                            await _userManager.AddToRolesAsync(user, rolesToAdd);
                        }

                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Cập nhật tài khoản nhân viên thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại dữ liệu
            var userForReload = await _userManager.FindByIdAsync(id);
            var staffForReload = await _context.Staffs.FindAsync(model.StaffId);
            
            ViewBag.UserRoles = userForReload != null ? await _userManager.GetRolesAsync(userForReload) : new List<string>();
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            model.Staff = staffForReload ?? new Staff();

            return View(model);
        }
    }

    // ViewModels for backward compatibility - should use Models namespace instead
    public class StaffAccountViewModel
    {
        public ApplicationUser User { get; set; }
        public Staff Staff { get; set; }
        public IList<string> Roles { get; set; }
    }
}
