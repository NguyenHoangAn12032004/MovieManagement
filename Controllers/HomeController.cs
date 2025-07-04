using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MovieManagement.Models;
using MovieManagement.Services;

namespace MovieManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMovieService _movieService;
    private readonly IMovieFilterService _movieFilterService;

    public HomeController(ILogger<HomeController> logger, IMovieService movieService, IMovieFilterService movieFilterService)
    {
        _logger = logger;
        _movieService = movieService;
        _movieFilterService = movieFilterService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Loading Home/Index page");
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            var featuredMovies = showingMovies.Where(m => m.IsFeatured).ToList();

            var viewModel = new MovieListViewModel
            {
                NowShowingMovies = showingMovies,
                ComingSoonMovies = comingSoonMovies,
                FeaturedMovies = featuredMovies
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index action");
            return View("Error");
        }
    }

    public async Task<IActionResult> Movies(string? searchTerm, string? genre, string? language)
    {
        var movies = await _movieFilterService.GetFilteredMoviesAsync(searchTerm, genre, language);
        return View(movies);
    }

    public async Task<IActionResult> MovieDetails(int id)
    {
        var showingMovies = await _movieService.GetShowingMoviesAsync();
        var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
        var allMovies = showingMovies.Concat(comingSoonMovies).ToList();
        
        var movie = allMovies.FirstOrDefault(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        var showtimes = await _movieService.GetShowtimesAsync(id);
        ViewBag.Showtimes = showtimes;

        return View(movie);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Tạo endpoint để tạo hình ảnh placeholder
    public IActionResult GeneratePlaceholder(string text, string type = "poster")
    {
        int width = type == "poster" ? 500 : 1200;
        int height = type == "poster" ? 750 : 600;
        string color = type == "poster" ? "0d47a1" : "1565c0";

        string svg = $@"<svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>
            <rect width='{width}' height='{height}' fill='#{color}' />
            <text x='{width / 2}' y='{height / 2}' font-family='Arial' font-size='24' fill='white' text-anchor='middle'>{text}</text>
        </svg>";

        return Content(svg, "image/svg+xml; charset=utf-8");
    }
}

