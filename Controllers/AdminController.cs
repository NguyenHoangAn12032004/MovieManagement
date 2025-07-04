using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieManagement.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MovieManagement.Models;

namespace MovieManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IMovieService movieService, ILogger<AdminController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var showingMovies = await _movieService.GetShowingMoviesAsync();
                var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();

                var viewModel = new MovieListViewModel
                {
                    NowShowingMovies = showingMovies,
                    ComingSoonMovies = comingSoonMovies
                };

                return View(viewModel);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Admin/Index action");
                return View("Error");
            }
        }

        public async Task<IActionResult> ImportMovies()
        {
            try
            {
                await _movieService.ImportMoviesFromApi();
                TempData["Success"] = "Đã nhập phim từ TMDb thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing movies");
                TempData["Error"] = "Lỗi khi nhập phim: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 