using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MovieManagement.Models;
using System.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieManagement.Services
{
    public interface ISqlService
    {
        Task<List<Movie>> GetShowingMoviesAsync();
        Task<List<Movie>> GetComingSoonMoviesAsync();
        Task<List<Showtime>> GetShowtimesAsync(int movieId);
        Task<Movie> GetMovieByIdAsync(int id);
        Task AddMoviesAsync(List<Movie> movies);
    }

    public class SqlService : ISqlService
    {
        private readonly string _connectionString;

        public SqlService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Movie>> GetShowingMoviesAsync()
        {
            var movies = new List<Movie>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Movies WHERE IsNowShowing = 1 ORDER BY ReleaseDate DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            movies.Add(MapToMovie(reader));
                        }
                    }
                }
            }
            return movies;
        }

        public async Task<List<Movie>> GetComingSoonMoviesAsync()
        {
            var movies = new List<Movie>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Movies WHERE IsComingSoon = 1 ORDER BY ReleaseDate ASC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            movies.Add(MapToMovie(reader));
                        }
                    }
                }
            }
            return movies;
        }

        public async Task<List<Showtime>> GetShowtimesAsync(int movieId)
        {
            var showtimes = new List<Showtime>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT s.*, t.Name as TheaterName, t.Location as TheaterLocation 
                      FROM Showtimes s 
                      JOIN Theaters t ON s.TheaterId = t.Id 
                      WHERE s.MovieId = @MovieId 
                      ORDER BY s.ShowDateTime", connection))
                {
                    command.Parameters.AddWithValue("@MovieId", movieId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            showtimes.Add(MapToShowtime(reader));
                        }
                    }
                }
            }
            return showtimes;
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Movies WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapToMovie(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task AddMoviesAsync(List<Movie> movies)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var movie in movies)
                        {
                            // Kiểm tra xem phim đã tồn tại trong database chưa
                            bool movieExists = false;
                            using (var command = new SqlCommand(
                                "SELECT COUNT(1) FROM Movies WHERE Title = @Title", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Title", movie.Title);
                                int count = (int)await command.ExecuteScalarAsync();
                                movieExists = count > 0;
                            }

                            if (!movieExists)
                            {
                                // Thêm phim mới
                                using (var command = new SqlCommand(
                                    @"INSERT INTO Movies (Title, Overview, ReleaseDate, PosterPath, BackdropPath, 
                                    VoteAverage, Status, GenreNames, TrailerUrl, Cast, Director, IsNowShowing, 
                                    IsComingSoon, Language, Duration, CreatedAt, UpdatedAt)
                                    VALUES (@Title, @Overview, @ReleaseDate, @PosterPath, @BackdropPath, 
                                    @VoteAverage, @Status, @GenreNames, @TrailerUrl, @Cast, @Director, @IsNowShowing, 
                                    @IsComingSoon, @Language, @Duration, @CreatedAt, @UpdatedAt);
                                    SELECT SCOPE_IDENTITY();", connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Title", movie.Title);
                                    command.Parameters.AddWithValue("@Overview", movie.Overview ?? "");
                                    command.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);
                                    command.Parameters.AddWithValue("@PosterPath", movie.PosterPath ?? "");
                                    command.Parameters.AddWithValue("@BackdropPath", movie.BackdropPath ?? "");
                                    command.Parameters.AddWithValue("@VoteAverage", (double)movie.Rating);
                                    command.Parameters.AddWithValue("@Status", movie.Status ?? "");
                                    command.Parameters.AddWithValue("@GenreNames", movie.Genres ?? "");
                                    command.Parameters.AddWithValue("@TrailerUrl", movie.TrailerUrl ?? "");
                                    command.Parameters.AddWithValue("@Cast", movie.Actors ?? "");
                                    command.Parameters.AddWithValue("@Director", movie.Director ?? "");
                                    command.Parameters.AddWithValue("@IsNowShowing", movie.Status == "Now Showing");
                                    command.Parameters.AddWithValue("@IsComingSoon", movie.Status == "Coming Soon");
                                    command.Parameters.AddWithValue("@Language", "vi-VN");
                                    command.Parameters.AddWithValue("@Duration", movie.Runtime);
                                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                                    var movieId = await command.ExecuteScalarAsync();
                                    movie.Id = Convert.ToInt32(movieId);
                                }
                            }
                            else
                            {
                                // Cập nhật phim đã tồn tại
                                using (var command = new SqlCommand(
                                    @"UPDATE Movies SET 
                                    Overview = @Overview,
                                    PosterPath = @PosterPath,
                                    BackdropPath = @BackdropPath,
                                    VoteAverage = @VoteAverage,
                                    Status = @Status,
                                    GenreNames = @GenreNames,
                                    TrailerUrl = @TrailerUrl,
                                    Cast = @Cast,
                                    Director = @Director,
                                    IsNowShowing = @IsNowShowing,
                                    IsComingSoon = @IsComingSoon,
                                    Duration = @Duration,
                                    UpdatedAt = @UpdatedAt
                                    WHERE Title = @Title", connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Title", movie.Title);
                                    command.Parameters.AddWithValue("@Overview", movie.Overview ?? "");
                                    command.Parameters.AddWithValue("@PosterPath", movie.PosterPath ?? "");
                                    command.Parameters.AddWithValue("@BackdropPath", movie.BackdropPath ?? "");
                                    command.Parameters.AddWithValue("@VoteAverage", (double)movie.Rating);
                                    command.Parameters.AddWithValue("@Status", movie.Status ?? "");
                                    command.Parameters.AddWithValue("@GenreNames", movie.Genres ?? "");
                                    command.Parameters.AddWithValue("@TrailerUrl", movie.TrailerUrl ?? "");
                                    command.Parameters.AddWithValue("@Cast", movie.Actors ?? "");
                                    command.Parameters.AddWithValue("@Director", movie.Director ?? "");
                                    command.Parameters.AddWithValue("@IsNowShowing", movie.Status == "Now Showing");
                                    command.Parameters.AddWithValue("@IsComingSoon", movie.Status == "Coming Soon");
                                    command.Parameters.AddWithValue("@Duration", movie.Runtime);
                                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // Commit transaction nếu mọi thứ thành công
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Rollback nếu có lỗi
                        await transaction.RollbackAsync();
                        throw new Exception($"Failed to add movies to database: {ex.Message}", ex);
                    }
                }
            }
        }

        private Movie MapToMovie(SqlDataReader reader)
        {
            return new Movie
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TmdbId = reader.GetInt32(reader.GetOrdinal("TmdbId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Overview = reader.GetString(reader.GetOrdinal("Overview")),
                ReleaseDate = reader.GetDateTime(reader.GetOrdinal("ReleaseDate")),
                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                PosterPath = reader.GetString(reader.GetOrdinal("PosterPath")),
                BackdropPath = reader.GetString(reader.GetOrdinal("BackdropPath")),
                VoteAverage = reader.GetDouble(reader.GetOrdinal("VoteAverage")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                GenreIds = reader.GetString(reader.GetOrdinal("GenreIds")),
                GenreNames = reader.GetString(reader.GetOrdinal("GenreNames")),
                Language = reader.GetString(reader.GetOrdinal("Language")),
                TrailerUrl = reader.GetString(reader.GetOrdinal("TrailerUrl")),
                Cast = reader.GetString(reader.GetOrdinal("Cast")),
                Director = reader.GetString(reader.GetOrdinal("Director")),
                IsNowShowing = reader.GetBoolean(reader.GetOrdinal("IsNowShowing")),
                IsComingSoon = reader.GetBoolean(reader.GetOrdinal("IsComingSoon")),
                IsFeatured = reader.GetBoolean(reader.GetOrdinal("IsFeatured"))
            };
        }

        private Showtime MapToShowtime(SqlDataReader reader)
        {
            return new Showtime
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                MovieId = reader.GetInt32(reader.GetOrdinal("MovieId")),
                TheaterId = reader.GetInt32(reader.GetOrdinal("TheaterId")),
                ShowDateTime = reader.GetDateTime(reader.GetOrdinal("ShowDateTime")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                ScreenType = reader.GetString(reader.GetOrdinal("ScreenType"))
            };
        }
    }
} 