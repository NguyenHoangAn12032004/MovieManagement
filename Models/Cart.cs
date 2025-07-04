using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MovieManagement.Models;

public class Cart
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual List<CartItem> Items { get; set; } = new List<CartItem>();
} 