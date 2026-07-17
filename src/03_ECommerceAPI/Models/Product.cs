namespace _03_EcommerceAPI.Models;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public uint Version { get; private set; } // For optimistic concurrency control

    public Product(string name, string description, string? imageUrl, decimal price, int stock, Category category)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        Price = price;
        Stock = stock;
        CategoryId = category.Id;
        Category = category;
    }

    private Product() { } // EF Core

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0.");
        }

        if (quantity > Stock)
        {
            throw new InvalidOperationException("Insufficient stock for the requested quantity.");
        }

        Stock -= quantity;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0.");
        }

        Stock += quantity;
    }
}