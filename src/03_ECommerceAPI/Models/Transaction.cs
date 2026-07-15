namespace _03_EcommerceAPI.Models;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TransactionType Type { get; private set; } = TransactionType.Payment;
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }

    public Transaction(Wallet wallet, decimal amount, TransactionType type, Guid? orderId = null)
    {
        Id = Guid.NewGuid();
        WalletId = wallet.Id;
        Wallet = wallet;
        Amount = amount;
        Type = type;
        OrderId = orderId;
        CreatedAt = DateTime.UtcNow;
    }

    private Transaction() { } // EF Core
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Payment,
}