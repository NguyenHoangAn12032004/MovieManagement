using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MovieManagement.Data;
using MovieManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public MovieController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: Admin/Movie
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .OrderBy(m => m.Id)
                .ToListAsync();
            return View(movies);
        }

        // GET: Admin/Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Admin/Movie/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Admin/Movie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Overview,ReleaseDate,Duration,PosterPath,BackdropPath,GenreId,Status,Language,TrailerUrl,Cast,Director,IsNowShowing,IsComingSoon,IsFeatured")] Movie movie, string GenreNames)
        {
            if (ModelState.IsValid)
            {
                movie.CreatedAt = DateTime.UtcNow;
                movie.UpdatedAt = DateTime.UtcNow;
                
                // Handle GenreNames as simple string
                if (!string.IsNullOrEmpty(GenreNames))
                {
                    movie.GenreNames = GenreNames.Trim();
                    // Set GenreIds as empty for now (can be enhanced later if needed)
                    movie.GenreIds = "";
                }
                else
                {
                    movie.GenreNames = "";
                    movie.GenreIds = "";
                }
                
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // GET: Admin/Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (movie == null)
            {
                return NotFound();
            }
            
            var viewModel = new MovieEditViewModel
            {
                Movie = movie,
                AvailableGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync(),
                SelectedGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList()
            };
            
            // Keep the old single genre selection for backward compatibility
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            
            return View(viewModel);
        }

        // POST: Admin/Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieEditViewModel viewModel, string GenreNames)
        {
            if (id != viewModel.Movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movie = viewModel.Movie;
                    movie.UpdatedAt = DateTime.UtcNow;
                    
                    // Handle GenreNames as simple string
                    if (!string.IsNullOrEmpty(GenreNames))
                    {
                        movie.GenreNames = GenreNames.Trim();
                        // Set GenreIds as empty for now (can be enhanced later if needed)
                        movie.GenreIds = "";
                    }
                    else
                    {
                        movie.GenreNames = "";
                        movie.GenreIds = "";
                    }
                    
                    // Update movie basic info
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(viewModel.Movie.Id))
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
            
            // If we got this far, something failed, redisplay form
            viewModel.AvailableGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", viewModel.Movie.GenreId);
            return View(viewModel);
        }

        // GET: Admin/Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Admin/Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Change movie status (featured, now showing, coming soon)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusType, bool value)
        {
            var movie = await _context.Movies.FindAsync(id);
            
            if (movie == null)
            {
                return NotFound();
            }
            
            // Check for invalid state combinations
            if (value) // Only check when turning a status ON
            {
                if (statusType == "nowshowing" && movie.IsComingSoon)
                {
                    return Json(new { success = false, message = "Phim đang ở trạng thái 'Sắp chiếu' nên không thể đồng thời ở trạng thái 'Đang chiếu'" });
                }
                else if (statusType == "comingsoon" && movie.IsNowShowing)
                {
                    return Json(new { success = false, message = "Phim đang ở trạng thái 'Đang chiếu' nên không thể đồng thời ở trạng thái 'Sắp chiếu'" });
                }
            }
            
            switch (statusType)
            {
                case "featured":
                    movie.IsFeatured = value;
                    break;
                case "nowshowing":
                    movie.IsNowShowing = value;
                    break;
                case "comingsoon":
                    movie.IsComingSoon = value;
                    break;
                default:
                    return BadRequest("Invalid status type");
            }
            
            movie.UpdatedAt = DateTime.UtcNow;
            
            // Clear any cached movie data
            if (_memoryCache != null)
            {
                _memoryCache.Remove("ShowingMovies");
                _memoryCache.Remove("ComingSoonMovies");
                _memoryCache.Remove($"Movie_{id}");
            }
            
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Trạng thái đã được cập nhật thành công" });
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
} 