using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MovieManagement.Models;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var users = await _userManager.Users
                .Where(u => u.Email.Contains(term) || u.UserName.Contains(term))
                .Select(u => new { id = u.Id, email = u.Email })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }
    }
} 