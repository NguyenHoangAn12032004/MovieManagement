using Microsoft.AspNetCore.Mvc;
using MovieManagement.Models;
using MovieManagement.Services;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;

namespace MovieManagement.Controllers;

public class MovieController : Controller
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MovieController> _logger;
    private readonly ApplicationDbContext _context;

    public MovieController(
        IMovieService movieService,
        ILogger<MovieController> logger,
        ApplicationDbContext context)
    {
        _movieService = movieService;
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(int? genreId)
    {
        try
        {
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            var allMovies = showingMovies.Concat(comingSoonMovies).ToList();
            
            // Get all genres for filter dropdown
            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Genres = genres;
            ViewBag.SelectedGenreId = genreId;
            
            // Filter by genre if specified
            if (genreId.HasValue)
            {
                allMovies = allMovies.Where(m => 
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(m.GenreIds))
                        {
                            var genreIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(m.GenreIds);
                            return genreIds != null && genreIds.Contains(genreId.Value);
                        }
                        return m.GenreId == genreId.Value;
                    }
                    catch
                    {
                        return m.GenreId == genreId.Value;
                    }
                }).ToList();
            }
            
            return View(allMovies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Movie/Index action");
            return View("Error");
        }
    }

    public async Task<IActionResult> NowShowing()
    {
        try
        {
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            return View(showingMovies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Movie/NowShowing action");
            return View("Error");
        }
    }

    public async Task<IActionResult> ComingSoon()
    {
        try
        {
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            return View(comingSoonMovies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Movie/ComingSoon action");
            return View("Error");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Movie/Details action");
            return View("Error");
        }
    }

    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return RedirectToAction(nameof(Index));
        }

        var movies = await _context.Movies
            .Where(m => m.Title.Contains(query) || 
                       m.Overview.Contains(query) ||
                       m.GenreNames.Contains(query))
            .Include(m => m.Genre)
            .ToListAsync();

        ViewBag.SearchQuery = query;
        return View("Index", movies);
    }

    public async Task<IActionResult> AllMovies()
    {
        try
        {
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            var allMovies = showingMovies.Concat(comingSoonMovies).ToList();
            return View(allMovies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Movie/AllMovies action");
            return View("Error");
        }
    }
}

