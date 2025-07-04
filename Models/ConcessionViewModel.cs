using System.Collections.Generic;

namespace MovieManagement.Models
{
    public class ConcessionViewModel
    {
        public List<Concession> Foods { get; set; } = new List<Concession>();
        public List<Concession> Drinks { get; set; } = new List<Concession>();
        public List<Concession> Combos { get; set; } = new List<Concession>();
        public Dictionary<int, int> SelectedItems { get; set; } = new Dictionary<int, int>();
        public decimal TotalAmount { get; set; }
        public string ReturnUrl { get; set; } = "/";
        public bool IsComboWithTicket { get; set; }
        public int? ShowtimeId { get; set; }
        public string? SelectedSeats { get; set; }
    }
}
