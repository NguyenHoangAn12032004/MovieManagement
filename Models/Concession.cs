using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class Concession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public ConcessionType Type { get; set; }
        
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;
        
        public bool IsAvailable { get; set; } = true;
        
        public bool IsCombo { get; set; } = false;
        
        public virtual ICollection<ConcessionItem> ConcessionItems { get; set; } = new List<ConcessionItem>();
    }
    
    public enum ConcessionType
    {
        Food,
        Drink,
        Combo
    }
}
