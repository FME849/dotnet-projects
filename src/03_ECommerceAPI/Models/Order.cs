namespace _03_EcommerceAPI.Models;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public decimal TotalPrice { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    private readonly List<OrderItem> _orderItems = new List<OrderItem>();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Order(User user)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;
        OrderDate = DateTime.UtcNow;
    }

    private Order() { } // EF Core

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0.");
        }

        var orderItem = new OrderItem(this, product, quantity);
        _orderItems.Add(orderItem);
        TotalPrice += orderItem.TotalPrice;
    }
}

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}