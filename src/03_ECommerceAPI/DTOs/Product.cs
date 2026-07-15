namespace _03_EcommerceAPI.DTOs;

public record CreateProductDto(string Name, string Description, string? ImageUrl, decimal Price, int Stock, Guid CategoryId);