using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieManagement.Models
{
    public class ConcessionItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CartItemId { get; set; }
        
        [Required]
        public int ConcessionId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => (Price * Quantity) - Discount;
        
        public bool IsComboItem { get; set; }
        
        [ForeignKey("CartItemId")]
        public virtual CartItem CartItem { get; set; } = null!;
        
        [ForeignKey("ConcessionId")]
        public virtual Concession Concession { get; set; } = null!;
    }
}
