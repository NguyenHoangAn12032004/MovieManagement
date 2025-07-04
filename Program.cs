using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MovieManagement.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using MovieManagement.Hubs;
using MovieManagement.Models;

namespace MovieTicketBooking
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            builder.Services.AddRazorPages();

            // Add SignalR
            builder.Services.AddSignalR();

            // Add localization
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Configure supported cultures
            var supportedCultures = new[]
            {
                new CultureInfo("vi"),
                new CultureInfo("en")
            };

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("vi");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // Add HttpClient and Movie Services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<ISqlService, SqlService>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IMovieFilterService, MovieFilterService>();

            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();            // Add Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = false;  // Không yêu cầu xác nhận email
                options.SignIn.RequireConfirmedEmail = false;    // Không yêu cầu xác nhận email
                options.SignIn.RequireConfirmedPhoneNumber = false; // Không yêu cầu xác nhận phone
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // Add Google Authentication
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                    options.CallbackPath = "/signin-google";
                    options.SaveTokens = true;
                });

            // Configure role-based authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
                options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
                options.AddPolicy("RequireAdminOrStaffRole", policy => policy.RequireRole("Admin", "Staff"));
            });            // Add Email Sender Service
            var useDeveloperEmailSender = builder.Configuration.GetValue<bool>("EmailSettings:UseDeveloperEmailSender");
            if (useDeveloperEmailSender)
            {
                builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, DeveloperEmailSender>();
            }
            else
            {
                builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
            }
            builder.Services.AddTransient<SmtpTestService>();

            // Add Session support
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Thêm logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Configure localization at the start of the pipeline
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("vi"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.MapRazorPages();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Đảm bảo thư mục images tồn tại
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable Session
            app.UseSession();

            // Thêm endpoint để tạo placeholder images
            app.MapGet("/images/placeholder/{type}/{name}", (string type, string name) => {
                int width = type == "poster" ? 500 : 1200;
                int height = type == "poster" ? 750 : 600;
                string color = type == "poster" ? "0d47a1" : "1565c0";

                string svg = $@"<svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>
                    <rect width='{width}' height='{height}' fill='#{color}' />
                    <text x='{width / 2}' y='{height / 2}' font-family='Arial' font-size='24' fill='white' text-anchor='middle'>{name}</text>
                </svg>";

                return Results.Text(svg, "image/svg+xml; charset=utf-8");
            });

            app.MapControllerRoute(
                name: "booking",
                pattern: "Booking/{action}/{id?}",
                defaults: new { controller = "Booking" });

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Configure SignalR hub endpoints
            app.MapHub<SeatHub>("/seathub");

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await DbSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
                
                // Seed concession data
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                await ConcessionSeeder.SeedConcessions(dbContext);
            }

            app.Run();
        }
    }
}

