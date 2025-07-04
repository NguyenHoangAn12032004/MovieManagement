using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class MovieGenre
    {
        public int MovieId { get; set; }
        public int GenreId { get; set; }

        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; }
    }
} 