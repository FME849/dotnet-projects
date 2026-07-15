namespace _03_EcommerceAPI.Models;

public class Wallet
{
    public Guid Id { get; private set; }
    public decimal Balance { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Wallet(decimal balance, User user)
    {
        Id = Guid.NewGuid();
        Balance = balance;
        UserId = user.Id;
        User = user;
    }

    private Wallet() { } // EF Core

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.");
        }

        Balance += amount;
        var _transaction = new Transaction(this, amount, TransactionType.Deposit);
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero.");
        }

        if (amount > Balance)
        {
            throw new InvalidOperationException("Insufficient balance for withdrawal.");
        }

        Balance -= amount;
        var _transaction = new Transaction(this, -amount, TransactionType.Withdrawal);
    }
}