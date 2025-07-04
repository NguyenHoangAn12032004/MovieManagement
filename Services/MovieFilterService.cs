using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using Microsoft.Extensions.Logging;
using MovieManagement.Services;

namespace MovieManagement.Services
{
    public interface IMovieFilterService
    {
        Task<List<Movie>> GetFilteredMoviesAsync(string? searchTerm = null, string? genre = null, string? language = null);
        Task<List<Movie>> GetNowShowingMoviesAsync();
        Task<List<Movie>> GetComingSoonMoviesAsync();
    }

    public class MovieFilterService : IMovieFilterService
    {
        private readonly IMovieService _movieService;

        public MovieFilterService(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task<List<Movie>> GetFilteredMoviesAsync(string? searchTerm = null, string? genre = null, string? language = null)
        {
            var showingMovies = await _movieService.GetShowingMoviesAsync();
            var comingSoonMovies = await _movieService.GetComingSoonMoviesAsync();
            var allMovies = showingMovies.Concat(comingSoonMovies).ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                allMovies = allMovies.Where(m =>
                    m.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Overview.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Cast.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Director.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.GenreNames.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                allMovies = allMovies.Where(m =>
                    m.GenreNames.Contains(genre, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                allMovies = allMovies.Where(m =>
                    m.Language.Equals(language, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return allMovies;
        }

        public async Task<List<Movie>> GetNowShowingMoviesAsync()
        {
            return await _movieService.GetShowingMoviesAsync();
        }

        public async Task<List<Movie>> GetComingSoonMoviesAsync()
        {
            return await _movieService.GetComingSoonMoviesAsync();
        }
    }
} 