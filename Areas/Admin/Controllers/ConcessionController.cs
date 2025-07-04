using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ConcessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConcessionController> _logger;

        public ConcessionController(ApplicationDbContext context, ILogger<ConcessionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Concession
        public async Task<IActionResult> Index()
        {
            var concessions = await _context.Concessions
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(concessions);
        }

        // GET: Admin/Concession/TestIndex (For testing with sample data)
        public IActionResult TestIndex()
        {
            // Sample data for testing the UI
            var sampleConcessions = new List<Concession>
            {
                new Concession { Id = 1, Name = "Bắp Caramel Lớn", Description = "Bắp rang caramel, size lớn (L)", Price = 89000, IsAvailable = true, ImagePath = "/images/concessions/caramel-large.jpg" },
                new Concession { Id = 2, Name = "Bắp Caramel Nhỏ", Description = "Bắp rang caramel, size nhỏ (M)", Price = 65000, IsAvailable = true, ImagePath = "/images/concessions/caramel-medium.jpg" },
                new Concession { Id = 3, Name = "Bắp Ngọt Lớn", Description = "Bắp rang ngọt, size lớn (L)", Price = 79000, IsAvailable = true, ImagePath = "/images/concessions/sweet-large.jpg" },
                new Concession { Id = 4, Name = "Bắp Ngọt Nhỏ", Description = "Bắp rang ngọt, size nhỏ (M)", Price = 55000, IsAvailable = true, ImagePath = "/images/concessions/sweet-medium.jpg" },
                new Concession { Id = 5, Name = "Coca-Cola Lớn", Description = "Coca-Cola size lớn (L)", Price = 45000, IsAvailable = true, ImagePath = "/images/concessions/coke-large.jpg" },
                new Concession { Id = 6, Name = "Coca-Cola Nhỏ", Description = "Coca-Cola size nhỏ (M)", Price = 35000, IsAvailable = true, ImagePath = "/images/concessions/coke-medium.jpg" },
                new Concession { Id = 7, Name = "Combo Caramel", Description = "1 bắp caramel lớn + 2 nước ngọt lớn", Price = 159000, IsAvailable = true, ImagePath = "/images/concessions/combo-caramel.jpg" },
                new Concession { Id = 8, Name = "Combo Đôi", Description = "1 bắp ngọt lớn + 2 nước ngọt lớn", Price = 139000, IsAvailable = true, ImagePath = "/images/concessions/combo-duo.jpg" },
                new Concession { Id = 9, Name = "Combo Gia Đình", Description = "2 bắp ngọt lớn + 4 nước ngọt nhỏ", Price = 249000, IsAvailable = true, ImagePath = "/images/concessions/combo-family.jpg" },
                new Concession { Id = 10, Name = "Combo Tiệt Kiệm", Description = "1 bắp ngọt nhỏ + 2 nước ngọt nhỏ", Price = 119000, IsAvailable = true, ImagePath = "/images/concessions/combo-saver.jpg" },
                new Concession { Id = 11, Name = "Sprite Lớn", Description = "Sprite, size lớn (L)", Price = 42000, IsAvailable = true, ImagePath = "/images/concessions/sprite-large.jpg" },
                new Concession { Id = 12, Name = "Sprite Nhỏ", Description = "Sprite, size nhỏ (M)", Price = 32000, IsAvailable = false, ImagePath = "/images/concessions/sprite-medium.jpg" }
            };

            return View("Index", sampleConcessions);
        }

        // GET: Admin/Concession/PublicTest (No authorization required)
        [AllowAnonymous]
        public IActionResult PublicTest()
        {
            // Sample data for testing the UI without authentication
            var sampleConcessions = new List<Concession>
            {
                new Concession { Id = 1, Name = "Bắp Caramel Lớn", Description = "Bắp rang caramel, size lớn (L)", Price = 89000, IsAvailable = true, ImagePath = "/images/concessions/caramel-large.jpg" },
                new Concession { Id = 2, Name = "Bắp Caramel Nhỏ", Description = "Bắp rang caramel, size nhỏ (M)", Price = 65000, IsAvailable = true, ImagePath = "/images/concessions/caramel-medium.jpg" },
                new Concession { Id = 3, Name = "Bắp Ngọt Lớn", Description = "Bắp rang ngọt, size lớn (L)", Price = 79000, IsAvailable = true, ImagePath = "/images/concessions/sweet-large.jpg" },
                new Concession { Id = 4, Name = "Bắp Ngọt Nhỏ", Description = "Bắp rang ngọt, size nhỏ (M)", Price = 55000, IsAvailable = true, ImagePath = "/images/concessions/sweet-medium.jpg" },
                new Concession { Id = 5, Name = "Coca-Cola Lớn", Description = "Coca-Cola size lớn (L)", Price = 45000, IsAvailable = true, ImagePath = "/images/concessions/coke-large.jpg" },
                new Concession { Id = 6, Name = "Coca-Cola Nhỏ", Description = "Coca-Cola size nhỏ (M)", Price = 35000, IsAvailable = true, ImagePath = "/images/concessions/coke-medium.jpg" },
                new Concession { Id = 7, Name = "Combo Caramel", Description = "1 bắp caramel lớn + 2 nước ngọt lớn", Price = 159000, IsAvailable = true, ImagePath = "/images/concessions/combo-caramel.jpg" },
                new Concession { Id = 8, Name = "Combo Đôi", Description = "1 bắp ngọt lớn + 2 nước ngọt lớn", Price = 139000, IsAvailable = true, ImagePath = "/images/concessions/combo-duo.jpg" },
                new Concession { Id = 9, Name = "Combo Gia Đình", Description = "2 bắp ngọt lớn + 4 nước ngọt nhỏ", Price = 249000, IsAvailable = true, ImagePath = "/images/concessions/combo-family.jpg" },
                new Concession { Id = 10, Name = "Combo Tiệt Kiệm", Description = "1 bắp ngọt nhỏ + 2 nước ngọt nhỏ", Price = 119000, IsAvailable = true, ImagePath = "/images/concessions/combo-saver.jpg" },
                new Concession { Id = 11, Name = "Sprite Lớn", Description = "Sprite, size lớn (L)", Price = 42000, IsAvailable = true, ImagePath = "/images/concessions/sprite-large.jpg" },
                new Concession { Id = 12, Name = "Sprite Nhỏ", Description = "Sprite, size nhỏ (M)", Price = 32000, IsAvailable = false, ImagePath = "/images/concessions/sprite-medium.jpg" }
            };

            return View("Index", sampleConcessions);
        }

        // GET: Admin/Concession/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concession = await _context.Concessions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (concession == null)
            {
                return NotFound();
            }

            return View(concession);
        }

        // GET: Admin/Concession/Create
        public IActionResult Create()
        {
            return View();
        }        // POST: Admin/Concession/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,IsAvailable,ImagePath")] Concession concession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(concession);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(concession);
        }

        // GET: Admin/Concession/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concession = await _context.Concessions.FindAsync(id);
            if (concession == null)
            {
                return NotFound();
            }
            return View(concession);
        }        // POST: Admin/Concession/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,IsAvailable,ImagePath")] Concession concession)
        {
            if (id != concession.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(concession);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConcessionExists(concession.Id))
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
            return View(concession);
        }

        // GET: Admin/Concession/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concession = await _context.Concessions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (concession == null)
            {
                return NotFound();
            }

            return View(concession);
        }

        // POST: Admin/Concession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var concession = await _context.Concessions.FindAsync(id);
            if (concession != null)
            {
                _context.Concessions.Remove(concession);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Concession/Statistics
        public async Task<IActionResult> Statistics()
        {
            var statistics = await GetConcessionStatistics();
            return View(statistics);
        }

        // API endpoint for getting statistics data
        [HttpGet]
        public async Task<IActionResult> GetStatisticsData()
        {
            var statistics = await GetConcessionStatistics();
            return Json(statistics);
        }

        private async Task<ConcessionStatisticsViewModel> GetConcessionStatistics()
        {
            var startDate = DateTime.Now.AddDays(-30); // Last 30 days
            var endDate = DateTime.Now;

            // Get concession sales data
            var concessionSales = await _context.ConcessionItems
                .Include(ci => ci.Concession)
                .Include(ci => ci.CartItem)
                .Where(ci => ci.CartItem.CreatedAt >= startDate && ci.CartItem.CreatedAt <= endDate)
                .Where(ci => ci.CartItem.Status == "Completed")
                .GroupBy(ci => ci.Concession.Name)
                .Select(g => new ConcessionSalesData
                {
                    Name = g.Key,
                    Quantity = g.Sum(ci => ci.Quantity),
                    Revenue = g.Sum(ci => ci.Quantity * ci.Price)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            // Get daily sales data for chart
            var dailySales = await _context.ConcessionItems
                .Include(ci => ci.CartItem)
                .Where(ci => ci.CartItem.CreatedAt >= startDate && ci.CartItem.CreatedAt <= endDate)
                .Where(ci => ci.CartItem.Status == "Completed")
                .GroupBy(ci => ci.CartItem.CreatedAt.Date)
                .Select(g => new DailySalesData
                {
                    Date = g.Key,
                    Revenue = g.Sum(ci => ci.Quantity * ci.Price),
                    Quantity = g.Sum(ci => ci.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Get top selling items
            var topItems = concessionSales.Take(5).ToList();

            // Calculate totals
            var totalRevenue = concessionSales.Sum(x => x.Revenue);
            var totalQuantity = concessionSales.Sum(x => x.Quantity);

            return new ConcessionStatisticsViewModel
            {
                ConcessionSales = concessionSales,
                DailySales = dailySales,
                TopSellingItems = topItems,
                TotalRevenue = totalRevenue,
                TotalQuantitySold = totalQuantity,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        private bool ConcessionExists(int id)
        {
            return _context.Concessions.Any(e => e.Id == id);
        }
    }

    // ViewModels for statistics
    public class ConcessionStatisticsViewModel
    {
        public List<ConcessionSalesData> ConcessionSales { get; set; } = new();
        public List<DailySalesData> DailySales { get; set; } = new();
        public List<ConcessionSalesData> TopSellingItems { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalQuantitySold { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ConcessionSalesData
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Quantity { get; set; }
    }
}
