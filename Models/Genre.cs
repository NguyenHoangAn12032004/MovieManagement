using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models;

public class Genre
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string Name { get; set; }
    
    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}

