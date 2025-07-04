using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using MovieManagement.Models;

namespace MovieManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Seed Roles
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Staff"));

            // Seed Admin User
            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user == null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
} 