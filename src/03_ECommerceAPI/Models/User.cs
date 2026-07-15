namespace _03_EcommerceAPI.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    public User(string name, string email, string password)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Password = password;
        Wallet = new Wallet(0, this); // Initialize wallet with zero balance
    }

    private User() { } // EF Core
}