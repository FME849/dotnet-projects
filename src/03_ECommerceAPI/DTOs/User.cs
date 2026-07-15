namespace _03_EcommerceAPI.DTOs;

public record CreateUserDto(string Name, string Email, string Password);
public record ResponseUserDto(Guid Id, string Name, string Email, string? AvatarUrl, decimal Balance);