using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Models;
using MovieManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy tất cả users trừ Admin
                var allUsers = await _userManager.Users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();
                var users = new List<ApplicationUser>();
                var userRoles = new Dictionary<string, IList<string>>();

                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles[user.Id] = roles;
                    
                    // Chỉ hiển thị User và Staff (không hiển thị Admin)
                    if (roles.Contains("User") || roles.Contains("Staff"))
                    {
                        users.Add(user);
                    }
                }

                ViewBag.UserRoles = userRoles;
                ViewBag.TotalUsers = users.Count;
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách người dùng: " + ex.Message;
                return View(new List<ApplicationUser>());
            }
        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(string id)
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

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            
            return View(user);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // Log khi vào trang Edit
            System.Diagnostics.Debug.WriteLine($"GET Edit action called with ID: {id}");
            
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await SetupViewBagData(user);
            
            // Log data được truyền vào view
            var roles = ViewBag.Roles as List<IdentityRole>;
            var userRoles = ViewBag.UserRoles as List<string>;
            System.Diagnostics.Debug.WriteLine($"Available Roles: {string.Join(", ", roles?.Select(r => r.Name) ?? new List<string>())}");
            System.Diagnostics.Debug.WriteLine($"User Roles: {string.Join(", ", userRoles ?? new List<string>())}");
            
            return View(user);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user, IFormCollection form)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"POST Edit action called with ID: {id}");
            System.Diagnostics.Debug.WriteLine($"User: {user?.UserName}, {user?.Email}");
            System.Diagnostics.Debug.WriteLine($"Form keys: {string.Join(", ", form.Keys)}");
            
            // Validate input parameters
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID tài khoản không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (id != user.Id)
            {
                TempData["Error"] = "ID tài khoản không khớp.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Extract selected roles from form data safely
                var selectedRoles = new List<string>();
                if (form.ContainsKey("selectedRoles"))
                {
                    var roleValues = form["selectedRoles"];
                    selectedRoles = roleValues
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();
                }

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Edit POST - User ID: {user.Id}");
                System.Diagnostics.Debug.WriteLine($"Form contains selectedRoles: {form.ContainsKey("selectedRoles")}");
                System.Diagnostics.Debug.WriteLine($"Raw form data: {string.Join(", ", form["selectedRoles"])}");
                System.Diagnostics.Debug.WriteLine($"Processed Selected Roles: {string.Join(", ", selectedRoles)}");

                // Get existing user from database
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser == null)
                {
                    TempData["Error"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction(nameof(Index));
                }

                // Clear ModelState for fields we don't want to validate from form
                ModelState.Remove("ConcurrencyStamp");
                ModelState.Remove("SecurityStamp");
                ModelState.Remove("PasswordHash");
                ModelState.Remove("NormalizedUserName");
                ModelState.Remove("NormalizedEmail");
                ModelState.Remove("EmailConfirmed");
                ModelState.Remove("LockoutEnabled");
                ModelState.Remove("TwoFactorEnabled");
                ModelState.Remove("AccessFailedCount");
                ModelState.Remove("LockoutEnd");
                ModelState.Remove("PhoneNumberConfirmed");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("FullName");
                ModelState.Remove("Bookings");

                // Validate model state for basic user properties
                // TEMPORARILY SKIP MODELSTATE VALIDATION FOR DEBUGGING
                if (false && !ModelState.IsValid)
                {
                    // Debug log for ModelState errors
                    var modelErrors = new List<string>();
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            modelErrors.Add($"{modelState.Key}: {error.ErrorMessage}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"ModelState errors: {string.Join("; ", modelErrors)}");
                    
                    // Also add the detailed errors to TempData for debugging
                    TempData["DetailedError"] = string.Join("; ", modelErrors);
                    
                    await SetupViewBagData(existingUser);
                    TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại thông tin.";
                    return View(user);
                }

                // Validate selected roles exist
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                var invalidRoles = selectedRoles.Where(r => !allRoles.Contains(r)).ToList();
                if (invalidRoles.Any())
                {
                    TempData["Error"] = $"Vai trò không hợp lệ: {string.Join(", ", invalidRoles)}";
                    await SetupViewBagData(existingUser);
                    return View(user);
                }

                // Update user basic properties
                existingUser.UserName = user.UserName?.Trim();
                existingUser.Email = user.Email?.Trim();
                existingUser.FirstName = !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName.Trim() : existingUser.FirstName;
                existingUser.LastName = !string.IsNullOrWhiteSpace(user.LastName) ? user.LastName.Trim() : existingUser.LastName;
                existingUser.PhoneNumber = user.PhoneNumber?.Trim();

                // Validate email uniqueness if changed
                if (existingUser.Email != user.Email?.Trim())
                {
                    var emailExists = await _userManager.FindByEmailAsync(user.Email?.Trim() ?? "");
                    if (emailExists != null && emailExists.Id != existingUser.Id)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                        await SetupViewBagData(existingUser);
                        TempData["Error"] = "Email đã tồn tại.";
                        return View(user);
                    }
                }

                // Validate username uniqueness if changed
                if (existingUser.UserName != user.UserName?.Trim())
                {
                    var usernameExists = await _userManager.FindByNameAsync(user.UserName?.Trim() ?? "");
                    if (usernameExists != null && usernameExists.Id != existingUser.Id)
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng.");
                        await SetupViewBagData(existingUser);
                        TempData["Error"] = "Tên đăng nhập đã tồn tại.";
                        return View(user);
                    }
                }

                // Update user properties
                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    
                    await SetupViewBagData(existingUser);
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin tài khoản.";
                    return View(user);
                }

                // Update user roles
                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                var rolesToAdd = selectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
                var rolesToRemove = currentRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).ToList();

                // Debug logging for roles
                System.Diagnostics.Debug.WriteLine($"Current Roles: {string.Join(", ", currentRoles)}");
                System.Diagnostics.Debug.WriteLine($"Roles to Add: {string.Join(", ", rolesToAdd)}");
                System.Diagnostics.Debug.WriteLine($"Roles to Remove: {string.Join(", ", rolesToRemove)}");

                // Remove roles that are no longer selected
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Lỗi khi xóa vai trò '{string.Join(", ", rolesToRemove)}': {error.Description}");
                        }
                        await SetupViewBagData(existingUser);
                        TempData["Error"] = "Có lỗi xảy ra khi cập nhật vai trò.";
                        return View(user);
                    }
                    System.Diagnostics.Debug.WriteLine($"Successfully removed roles: {string.Join(", ", rolesToRemove)}");
                }

                // Add new roles that were selected
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(existingUser, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        foreach (var error in addResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Lỗi khi thêm vai trò '{string.Join(", ", rolesToAdd)}': {error.Description}");
                        }
                        await SetupViewBagData(existingUser);
                        TempData["Error"] = "Có lỗi xảy ra khi cập nhật vai trò.";
                        return View(user);
                    }
                    System.Diagnostics.Debug.WriteLine($"Successfully added roles: {string.Join(", ", rolesToAdd)}");
                }

                // Success message
                var changesMessage = new List<string>();
                if (rolesToAdd.Any()) changesMessage.Add($"Thêm vai trò: {string.Join(", ", rolesToAdd)}");
                if (rolesToRemove.Any()) changesMessage.Add($"Xóa vai trò: {string.Join(", ", rolesToRemove)}");
                
                var successMsg = "Cập nhật tài khoản thành công!";
                if (changesMessage.Any())
                {
                    successMsg += $" ({string.Join("; ", changesMessage)})";
                }
                
                TempData["Success"] = successMsg;
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(user.Id))
                {
                    TempData["Error"] = "Tài khoản không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "Tài khoản đã được cập nhật bởi người khác. Vui lòng thử lại.";
                await SetupViewBagData(await _userManager.FindByIdAsync(user.Id));
                return View(user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Edit: {ex}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser != null)
                {
                    await SetupViewBagData(existingUser);
                    return View(user);
                }
                
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(string id)
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

            // Lấy vai trò của user
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy tài khoản cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem có phải admin không
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    TempData["Error"] = "Không thể xóa tài khoản Admin.";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa các dữ liệu liên quan trước khi xóa user
                await DeleteRelatedUserData(user.Id);

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Đã xóa tài khoản thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa tài khoản: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa tài khoản: " + ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPersonalData(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Chỉ lấy những thông tin cần thiết
            var personalData = new Dictionary<string, string>
            {
                { "UserId", user.Id },
                { "UserName", user.UserName },
                { "FirstName", user.FirstName },
                { "LastName", user.LastName },
                { "Email", user.Email },
                { "PhoneNumber", user.PhoneNumber ?? "Not set" },
                { "Roles", string.Join(", ", await _userManager.GetRolesAsync(user)) }
            };

            var json = JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", $"PersonalData-{user.UserName}.json");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePersonalData(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có phải admin không
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Đã xóa tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/User/Debug - Temporary method để debug role issues
        public async Task<IActionResult> Debug()
        {
            var debugInfo = new List<string>();
            
            try
            {
                // Lấy tất cả users
                var allUsers = await _userManager.Users.ToListAsync();
                debugInfo.Add($"Total users in database: {allUsers.Count}");
                
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    debugInfo.Add($"User: {user.UserName} ({user.Email}) - Roles: {string.Join(", ", roles)}");
                }
                
                // Lấy tất cả roles
                var allRoles = await _roleManager.Roles.ToListAsync();
                debugInfo.Add($"\nTotal roles in database: {allRoles.Count}");
                
                foreach (var role in allRoles)
                {
                    debugInfo.Add($"Role: {role.Name}");
                }
                
                ViewBag.DebugInfo = debugInfo;
                return View();
            }
            catch (Exception ex)
            {
                debugInfo.Add($"Error: {ex.Message}");
                ViewBag.DebugInfo = debugInfo;
                return View();
            }
        }

        private async Task DeleteRelatedUserData(string userId)
        {
            try
            {
                // Xóa Cart của user
                var userCarts = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
                if (userCarts.Any())
                {
                    _context.Carts.RemoveRange(userCarts);
                }

                // Xóa Booking của user
                var userBookings = await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
                if (userBookings.Any())
                {
                    // Xóa BookingSeat trước
                    foreach (var booking in userBookings)
                    {
                        var bookingSeats = await _context.BookingSeats.Where(bs => bs.BookingId == booking.Id).ToListAsync();
                        if (bookingSeats.Any())
                        {
                            _context.BookingSeats.RemoveRange(bookingSeats);
                        }
                    }
                    
                    _context.Bookings.RemoveRange(userBookings);
                }

                // Lưu thay đổi
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không gián đoạn quá trình xóa user
                System.Diagnostics.Debug.WriteLine($"Error deleting related user data: {ex.Message}");
            }
        }

        private async Task SetupViewBagData(ApplicationUser? user)
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = allRoles;
            
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles = userRoles;
            }
            else
            {
                ViewBag.UserRoles = new List<string>();
            }
        }

        private async Task<bool> UserExists(string id)
        {
            return await _userManager.FindByIdAsync(id) != null;
        }
    }
}