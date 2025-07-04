using System.ComponentModel.DataAnnotations;

namespace MovieManagement.Models
{
    public class MovieEditViewModel
    {
        public Movie Movie { get; set; } = new Movie 
        { 
            Title = "",
            ReleaseDate = DateTime.Now,
            Status = "Active"
        };
        public List<Genre> AvailableGenres { get; set; } = new List<Genre>();
        public List<int> SelectedGenreIds { get; set; } = new List<int>();
        
        // Properties to display selected genres for better UX
        public string SelectedGenreNames => string.Join(", ", 
            AvailableGenres.Where(g => SelectedGenreIds.Contains(g.Id)).Select(g => g.Name));
    }
}
