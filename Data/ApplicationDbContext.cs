using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieManagement.Models;

namespace MovieManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<Seat> Seats { get; set; } = null!;
        public DbSet<Showtime> Showtimes { get; set; } = null!;
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<StaffSchedule> StaffSchedules { get; set; } = null!;
        public DbSet<Concession> Concessions { get; set; } = null!;
        public DbSet<ConcessionItem> ConcessionItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser properties
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configure decimal properties
            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<CartItem>()
                .Property(ci => ci.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Seat>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Showtime>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            // Configure Staff decimal properties
            builder.Entity<Staff>()
                .Property(s => s.HourlyRate)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Staff>()
                .Property(s => s.Bonus)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Staff>()
                .Property(s => s.Penalty)
                .HasColumnType("decimal(18,2)");

            // Removing the Salary property mapping since it's a computed property
            // builder.Entity<Staff>()
            //    .Property(s => s.Salary)
            //    .HasColumnType("decimal(18,2)");

            // Configure relationships with NO ACTION delete behavior
            builder.Entity<Booking>()
                .HasOne(b => b.Showtime)
                .WithMany()
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);            builder.Entity<CartItem>()
                .HasOne(ci => ci.Showtime)
                .WithMany()
                .HasForeignKey(ci => ci.ShowtimeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany()
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Seat>()
                .HasOne(s => s.Showtime)
                .WithMany()
                .HasForeignKey(s => s.ShowtimeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure relationships
            builder.Entity<Showtime>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Showtime>()
                .HasOne(s => s.Theater)
                .WithMany()
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Theater entity
            builder.Entity<Theater>()
                .Property(t => t.Capacity)
                .HasDefaultValue(120);

            builder.Entity<Movie>()
                .HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey("GenreId")
                .IsRequired(false);

            builder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            builder.Entity<MovieGenre>()
                .HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId);            builder.Entity<MovieGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId);
                
            // Configure Concession relationships
            builder.Entity<ConcessionItem>()
                .HasOne(ci => ci.CartItem)
                .WithMany(ci => ci.ConcessionItems)
                .HasForeignKey(ci => ci.CartItemId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<ConcessionItem>()
                .HasOne(ci => ci.Concession)
                .WithMany(c => c.ConcessionItems)
                .HasForeignKey(ci => ci.ConcessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
