using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _03_EcommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly EcommerceDbContext _dbContext;

    public OrderService(EcommerceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order> CreateOrderAsync(Guid userId, List<CreateOrderItemDto> items)
    {
        int maxRetryAttempts = 3;
        int retryCount = 0;

        while (true)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var user = await _dbContext.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new ArgumentException($"Could not find user with id {userId}");
                }

                var order = new Order(user);
                var productIds = items.Select(dto => dto.ProductId);
                var products = await _dbContext.Products.Include(p => p.Category).Where(p => productIds.Contains(p.Id)).ToListAsync();

                if (products.Count < items.Count)
                {
                    var existingIds = new HashSet<Guid>(products.Select(p => p.Id));
                    var missingIds = productIds.Except(existingIds).ToList();
                    throw new ArgumentException($"Could not find products with ids ${missingIds}.");
                }

                foreach (var product in products)
                {
                    var quantity = items.FirstOrDefault(dto => dto.ProductId == product.Id)!.Quantity;
                    product.ReduceStock(quantity);
                    order.AddItem(product, quantity);
                }

                if (user.Wallet.Balance < order.TotalPrice)
                {
                    throw new InvalidOperationException("Wallet balance is not enough.");
                }

                _dbContext.Orders.Add(order);
                user.Wallet.Withdraw(order.TotalPrice);
                var payment = new Transaction(user.Wallet, -order.TotalPrice, TransactionType.Payment, order.Id);
                _dbContext.Transactions.Add(payment);
                var result = await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _dbContext.ChangeTracker.Clear();
                retryCount++;
                if (retryCount >= maxRetryAttempts)
                {
                    throw new DbUpdateConcurrencyException("Concurrency conflict occurred. Please try again later.", ex);
                }

                await Task.Delay(Random.Shared.Next(10, 50)); // Random delay to reduce contention. Avoid Thundering Herd problem
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
    }
}