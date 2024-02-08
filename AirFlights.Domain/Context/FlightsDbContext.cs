using Microsoft.EntityFrameworkCore;

namespace AirFlights.Domain;

public class FlightsDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public FlightsDbContext()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Code = "User" },
            new Role { Id = 2, Code = "Moderator" }
        );
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.RoleId);
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "User", Password = "eddef9e8e578c2a560c3187c4152c8b6f3f90c1dcf8c88b386ac1a9a96079c2c", RoleId = 1 },//Password - TestPass, Alg - SHA256
            new User { Id = 2, Username = "Admin", Password = "0161e13f3124ae3455747b1a9ed78aa231253ae5c543cd28b9a6605835148299", RoleId = 2 }//Password - AdminPass, Alg - SHA256
        );
        modelBuilder.Entity<Flight>().HasData(
            new Flight { Id = 1, Origin = "SVO", Destination = "AAQ", Departure = new DateTimeOffset(2024, 2, 8, 15, 0, 0, TimeSpan.FromHours(4)), Arrival = new DateTimeOffset(2024, 2, 8, 18, 30, 0, TimeSpan.FromHours(4)), Status = StatusEnum.InTime },
            new Flight { Id = 2, Origin = "DME", Destination = "AER", Departure = new DateTimeOffset(2024, 2, 9, 12, 45, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Cancelled },
            new Flight { Id = 3, Origin = "AER", Destination = "DME", Departure = new DateTimeOffset(2024, 2, 7, 11, 0, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Delayed },
            new Flight { Id = 4, Origin = "AAQ", Destination = "DME", Departure = new DateTimeOffset(2024, 2, 8, 16, 30, 0, TimeSpan.FromHours(4)), Arrival = new DateTimeOffset(2024, 2, 8, 19, 30, 0, TimeSpan.FromHours(4)), Status = StatusEnum.InTime },
            new Flight { Id = 5, Origin = "AER", Destination = "SVO", Departure = new DateTimeOffset(2024, 2, 1, 9, 0, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Cancelled },
            new Flight { Id = 6, Origin = "DME", Destination = "AER", Departure = new DateTimeOffset(2024, 2, 2, 7, 12, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Delayed },
            new Flight { Id = 7, Origin = "SVO", Destination = "AER", Departure = new DateTimeOffset(2024, 2, 8, 20, 0, 0, TimeSpan.FromHours(4)), Arrival = new DateTimeOffset(2024, 2, 8, 23, 30, 0, TimeSpan.FromHours(4)), Status = StatusEnum.InTime },
            new Flight { Id = 8, Origin = "AAQ", Destination = "SVO", Departure = new DateTimeOffset(2024, 2, 3, 1, 0, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Cancelled },
            new Flight { Id = 9, Origin = "AER", Destination = "SVO", Departure = new DateTimeOffset(2024, 2, 7, 5, 0, 0, TimeSpan.FromHours(4)), Arrival = null, Status = StatusEnum.Delayed },
            new Flight { Id = 10, Origin = "AAQ", Destination = "DME", Departure = new DateTimeOffset(2024, 2, 8, 12, 0, 0, TimeSpan.FromHours(4)), Arrival = new DateTimeOffset(2024, 2, 8, 15, 30, 0, TimeSpan.FromHours(4)), Status = StatusEnum.InTime }
        );
    }
}