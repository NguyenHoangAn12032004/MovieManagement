using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Models;

namespace MovieManagement.Data
{
    public static class ConcessionSeeder
    {
        public static async Task SeedConcessions(ApplicationDbContext context)
        {
            // Kiểm tra nếu đã có dữ liệu rồi thì không seed nữa
            if (await context.Concessions.AnyAsync())
            {
                return;
            }

            // Tạo danh sách các sản phẩm bắp nước
            var concessions = new List<Concession>
            {
                // Bắp
                new Concession 
                { 
                    Name = "Bắp Ngọt Nhỏ", 
                    Description = "Bắp rang bơ ngọt, size nhỏ (M)", 
                    Price = 45000M, 
                    Type = ConcessionType.Food,
                    ImagePath = "/images/concession/popcorn-small.jpg",
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Bắp Ngọt Lớn", 
                    Description = "Bắp rang bơ ngọt, size lớn (L)", 
                    Price = 55000M, 
                    Type = ConcessionType.Food,
                    ImagePath = "/images/concession/popcorn-large.jpg",
                    IsAvailable = true
                },
                new Concession                { 
                    Name = "Bắp Caramel Nhỏ", 
                    Description = "Bắp rang caramen, size nhỏ (M)", 
                    Price = 55000M, 
                    Type = ConcessionType.Food,
                    ImagePath = "/images/concession/popcorn-small.jpg", // Use existing image
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Bắp Caramel Lớn", 
                    Description = "Bắp rang caramen, size lớn (L)", 
                    Price = 65000M, 
                    Type = ConcessionType.Food,
                    ImagePath = "/images/concession/popcorn-large.jpg", // Use existing image
                    IsAvailable = true
                },
                
                // Nước uống
                new Concession 
                { 
                    Name = "Coca-Cola Nhỏ", 
                    Description = "Coca-Cola, size nhỏ (M)", 
                    Price = 35000M, 
                    Type = ConcessionType.Drink,
                    ImagePath = "/images/concession/coke-small.jpg",
                    IsAvailable = true
                },                new Concession 
                { 
                    Name = "Coca-Cola Lớn", 
                    Description = "Coca-Cola, size lớn (L)", 
                    Price = 45000M, 
                    Type = ConcessionType.Drink,
                    ImagePath = "/images/concession/coke-small.jpg", // Use existing image
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Sprite Nhỏ", 
                    Description = "Sprite, size nhỏ (M)", 
                    Price = 35000M, 
                    Type = ConcessionType.Drink,
                    ImagePath = "/images/concession/coke-small.jpg", // Use existing image
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Sprite Lớn", 
                    Description = "Sprite, size lớn (L)", 
                    Price = 45000M, 
                    Type = ConcessionType.Drink,
                    ImagePath = "/images/concession/coke-small.jpg", // Use existing image
                    IsAvailable = true
                },
                
                // Combo
                new Concession 
                { 
                    Name = "Combo Tiêu Chuẩn", 
                    Description = "1 bắp ngọt lớn + 2 nước ngọt nhỏ", 
                    Price = 115000M, 
                    Type = ConcessionType.Combo,
                    ImagePath = "/images/concession/combo-standard.jpg",
                    IsCombo = true,
                    IsAvailable = true
                },                new Concession 
                { 
                    Name = "Combo Đôi", 
                    Description = "1 bắp ngọt lớn + 2 nước ngọt lớn", 
                    Price = 135000M, 
                    Type = ConcessionType.Combo,
                    ImagePath = "/images/concession/combo-standard.jpg", // Use existing image
                    IsCombo = true,
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Combo Caramel", 
                    Description = "1 bắp caramel lớn + 2 nước ngọt nhỏ", 
                    Price = 125000M, 
                    Type = ConcessionType.Combo,
                    ImagePath = "/images/concession/combo-standard.jpg", // Use existing image
                    IsCombo = true,
                    IsAvailable = true
                },
                new Concession 
                { 
                    Name = "Combo Gia Đình", 
                    Description = "2 bắp ngọt lớn + 4 nước ngọt nhỏ", 
                    Price = 195000M, 
                    Type = ConcessionType.Combo,
                    ImagePath = "/images/concession/combo-standard.jpg", // Use existing image
                    IsCombo = true,
                    IsAvailable = true
                }
            };

            await context.Concessions.AddRangeAsync(concessions);
            await context.SaveChangesAsync();
        }
    }
}
