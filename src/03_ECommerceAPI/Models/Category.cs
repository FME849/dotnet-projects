namespace _03_EcommerceAPI.Models;

public class Category
{
    private readonly List<Product> _products = new List<Product>();
    public IReadOnlyList<Product> Products => _products.AsReadOnly();
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public Category(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    private Category() { } // EF Core
}