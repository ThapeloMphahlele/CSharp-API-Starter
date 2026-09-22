using BareBonesApi.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BareBonesApi.Entities;

public class ApplicationDbContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public ApplicationDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var provider = Configuration["DatabaseProvider"];

        if (provider == "MySQL")
        {
            var connectionString = Configuration.GetConnectionString("MySQL");
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }
        else if (provider == "SqlServer")
        {
            var connectionString = Configuration.GetConnectionString("SqlServer");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure RefreshToken Primary Key
        modelBuilder.Entity<RefreshToken>()
            .HasKey(rt => rt.Token);

        // One-to-Many Relationship with Cascade Delete
        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enforce unique constraints at the database level
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}