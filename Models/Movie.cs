using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieManagement.Models;

public class Movie : IValidatableObject
{
    public int Id { get; set; }
    
    public int TmdbId { get; set; }
    
    [Required]
    [StringLength(200)]
    [Column(TypeName = "nvarchar(200)")]
    public required string Title { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string Overview { get; set; } = string.Empty;
    
    [Required]
    public required DateTime ReleaseDate { get; set; }
    
    public int Duration { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string PosterPath { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string BackdropPath { get; set; } = string.Empty;
    
    public double VoteAverage { get; set; }
    
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public required string Status { get; set; } = "Active";
    
    [Column(TypeName = "nvarchar(max)")]
    public string GenreIds { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(max)")]
    public string GenreNames { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(50)")]
    public string Language { get; set; } = "Unknown";
    
    [Column(TypeName = "nvarchar(max)")]
    public string TrailerUrl { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Cast { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(100)")]
    public string Director { get; set; } = "Unknown";
    
    [Display(Name = "Đang chiếu")]
    public bool IsNowShowing { get; set; }
    
    [Display(Name = "Sắp chiếu")]
    public bool IsComingSoon { get; set; }
    
    [Display(Name = "Nổi bật")]
    public bool IsFeatured { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int? GenreId { get; set; }
    public Genre? Genre { get; set; }
    
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    [NotMapped]
    public string Actors { get => Cast; set => Cast = value; }
    
    [NotMapped]
    public string Genres { get => GenreNames; set => GenreNames = value; }
    
    [NotMapped]
    public double Rating { get => VoteAverage; set => VoteAverage = value; }
    
    [NotMapped]
    public int Runtime { get => Duration; set => Duration = value; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate that a movie cannot be both "Coming Soon" and "Now Showing"
        if (IsComingSoon && IsNowShowing)
        {
            yield return new ValidationResult(
                "Phim không thể vừa 'Đang chiếu' vừa 'Sắp chiếu'",
                new[] { nameof(IsComingSoon), nameof(IsNowShowing) });
        }
    }
}

public class MovieListViewModel
{
    public List<Movie> FeaturedMovies { get; set; } = new List<Movie>();
    public List<Movie> NowShowingMovies { get; set; } = new List<Movie>();
    public List<Movie> ComingSoonMovies { get; set; } = new List<Movie>();
}

