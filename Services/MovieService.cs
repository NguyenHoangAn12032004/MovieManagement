using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Data;
using MovieManagement.Models;
using System.Linq;
using MovieManagement.Services;

namespace MovieManagement.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetShowingMoviesAsync();
        Task<List<Movie>> GetComingSoonMoviesAsync();
        Task<List<Showtime>> GetShowtimesAsync(int movieId);
        Task<Movie> GetMovieByIdAsync(int id);
        Task ImportMoviesFromApi();
    }

    public class MovieService : IMovieService
    {
        private readonly ISqlService _sqlService;

        public MovieService(ISqlService sqlService)
        {
            _sqlService = sqlService;
        }

        public async Task<List<Movie>> GetShowingMoviesAsync()
        {
            return await _sqlService.GetShowingMoviesAsync();
        }

        public async Task<List<Movie>> GetComingSoonMoviesAsync()
        {
            return await _sqlService.GetComingSoonMoviesAsync();
        }

        public async Task<List<Showtime>> GetShowtimesAsync(int movieId)
        {
            return await _sqlService.GetShowtimesAsync(movieId);
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            return await _sqlService.GetMovieByIdAsync(id);
        }

        public async Task ImportMoviesFromApi()
        {
            // This method is no longer needed as we're using SQL data
            throw new NotImplementedException("ImportMoviesFromApi is no longer supported. Please use SQL data instead.");
        }
    }

    public class TmdbResponse
    {
        public List<MovieData> Results { get; set; }
    }

    public class MovieData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public double VoteAverage { get; set; }
        public DateTime ReleaseDate { get; set; }
    }

    public class MovieDetail : MovieData
    {
        public int Runtime { get; set; }
        public string Status { get; set; }
        public List<Genre> Genres { get; set; }
        public Credits Credits { get; set; }
        public Videos Videos { get; set; }
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Credits
    {
        public List<Cast> Cast { get; set; }
        public List<Crew> Crew { get; set; }
    }

    public class Cast
    {
        public string Name { get; set; }
        public string Character { get; set; }
    }

    public class Crew
    {
        public string Name { get; set; }
        public string Job { get; set; }
    }

    public class Videos
    {
        public List<Video> Results { get; set; }
    }

    public class Video
    {
        public string Key { get; set; }
        public string Site { get; set; }
        public string Type { get; set; }
    }
} 