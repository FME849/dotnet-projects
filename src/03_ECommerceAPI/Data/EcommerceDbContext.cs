using Microsoft.EntityFrameworkCore;
using _03_EcommerceAPI.Models;

namespace _03_EcommerceAPI.Data;

public class EcommerceDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(e => e.Wallet)
            .WithOne(e => e.User)
            .HasForeignKey<Wallet>(e => e.UserId)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.Version)
            .IsRowVersion(); // Configure Version as a concurrency token
    }
}