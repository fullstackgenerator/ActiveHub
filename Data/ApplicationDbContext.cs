using ActiveHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActiveHub.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<MembershipType> MembershipTypes { get; set; }
    public DbSet<Membership> Memberships { get; set; } 
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<MembershipType>().HasData(
            // regular memberships
            new MembershipType
            {
                Id = 1,
                Name = "Daily Pass",
                Description = "One-day access to all gym facilities",
                Price = 10.00m,
                Category = MembershipCategory.Regular,
                DurationInDays = 1
            },
            new MembershipType
            {
                Id = 2,
                Name = "Weekly Pass",
                Description = "Weekly access to all gym facilities",
                Price = 60.00m,
                Category = MembershipCategory.Regular,
                DurationInDays = 7
            },
            new MembershipType
            {
                Id = 3,
                Name = "Monthly Pass",
                Description = "Unlimited access for one month",
                Price = 190.00m,
                Category = MembershipCategory.Regular,
                DurationInDays = 31
            },
            new MembershipType
            {
                Id = 4,
                Name = "Yearly Pass",
                Description = "Unlimited access for one year",
                Price = 650.00m,
                Category = MembershipCategory.Regular,
                DurationInDays = 366
            },
        
            // special programs
            new MembershipType
            {
                Id = 5,
                Name = "Strength Training (2 Weeks)",
                Description = "Small-group strength training program with certified trainer",
                Price = 199.00m,
                Category = MembershipCategory.Special,
                DurationInDays = 14,
                MaxParticipants = 15
            },
            new MembershipType
            {
                Id = 6,
                Name = "HIIT Bootcamp (3 Weeks)",
                Description = "High-intensity interval training program",
                Price = 179.00m,
                Category = MembershipCategory.Special,
                DurationInDays = 21,
                MaxParticipants = 15
            }
        );
    }
}