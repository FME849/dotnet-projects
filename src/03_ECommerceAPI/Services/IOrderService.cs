using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;

namespace _03_EcommerceAPI.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid userId, List<CreateOrderItemDto> items);
}