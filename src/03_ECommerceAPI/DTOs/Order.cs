using _03_EcommerceAPI.Models;

namespace _03_EcommerceAPI.DTOs;

public record CreateOrderDto(Guid UserId, List<CreateOrderItemDto> Items);

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record ResponseOrderDto(Guid Id, decimal TotalPrice, List<ResponseOrderItemDto>Items);

public record ResponseOrderItemDto(Product product, int Quantity);