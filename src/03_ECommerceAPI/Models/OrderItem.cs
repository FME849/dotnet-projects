namespace _03_EcommerceAPI.Models;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal TotalPrice => Price * Quantity;

    public OrderItem(Order order, Product product, int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = order.Id;
        Order = order;
        ProductId = product.Id;
        Product = product;
        Quantity = quantity;
        Price = product.Price;
    }

    private OrderItem() { } // EF Core
}