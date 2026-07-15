using Microsoft.EntityFrameworkCore;
using _03_EcommerceAPI.Models;

namespace _03_EcommerceAPI.Data;

public class EcommerceDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(e => e.Wallet)
            .WithOne(e => e.User)
            .HasForeignKey<Wallet>(e => e.UserId)
            .IsRequired();
    }
}