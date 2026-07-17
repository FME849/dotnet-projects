using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _03_EcommerceAPI.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", CreateOrder);
        app.MapPost("/test-concurrency", TestConcurrency);
    }

    private static async Task<IResult> CreateOrder(CreateOrderDto createOrderDto, EcommerceDbContext dbContext)
    {
        int maxRetryAttempts = 3;
        int retryCount = 0;

        while (true)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var user = await dbContext.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == createOrderDto.UserId);
                if (user == null)
                {
                    throw new ArgumentException($"Could not find user with id {createOrderDto.UserId}");
                }

                var order = new Order(user);
                var productIds = createOrderDto.Items.Select(dto => dto.ProductId);
                var products = await dbContext.Products.Include(p => p.Category).Where(p => productIds.Contains(p.Id)).ToListAsync();

                if (products.Count < productIds.Count())
                {
                    var existingIds = new HashSet<Guid>(products.Select(p => p.Id));
                    var missingIds = productIds.Except(existingIds).ToList();
                    throw new ArgumentException($"Could not find products with ids ${missingIds}.");
                }

                foreach (var product in products)
                {
                    var quantity = createOrderDto.Items.FirstOrDefault(dto => dto.ProductId == product.Id)!.Quantity;
                    product.ReduceStock(quantity);
                    order.AddItem(product, quantity);
                }

                if (user.Wallet.Balance < order.TotalPrice)
                {
                    throw new ArgumentException("Wallet balance is not enough.");
                }

                dbContext.Orders.Add(order);
                user.Wallet.Withdraw(order.TotalPrice);
                var payment = new Transaction(user.Wallet, -order.TotalPrice, TransactionType.Payment, order.Id);
                dbContext.Transactions.Add(payment);
                var result = await dbContext.SaveChangesAsync();
                var responseOrderItems = order.OrderItems.Select(i => new ResponseOrderItemDto(i.Product.Id, i.Product.Name, i.Product.Price, i.Quantity)).ToList();
                var responseOrder = new ResponseOrderDto(order.Id, order.TotalPrice, responseOrderItems);
                await transaction.CommitAsync();
                return Results.Created($"/orders/{responseOrder.Id}", responseOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                retryCount++;
                if (retryCount >= maxRetryAttempts)
                {
                    return Results.Conflict("Concurrency conflict occurred. Please try again later.");
                }

                await Task.Delay(Random.Shared.Next(10, 50)); // Random delay to reduce contention
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Results.BadRequest(ex.Message);
            }
        }
    }

    private static async Task<IResult> TestConcurrency(HttpContext httpContext, IHttpClientFactory httpClientFactory, EcommerceDbContext dbContext)
    {
        const string email = "testuser@example.com";

        var user = await dbContext.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User("Test User", email, "password");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            if (user.Wallet.Balance > 0)
            {
                user.Wallet.Withdraw(user.Wallet.Balance);
            }
            user.Wallet.Deposit(10000);
            await dbContext.SaveChangesAsync();
        }

        const string productName = "Test Product";
        var product = await dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Name == productName);
        if (product == null)
        {
            var category = new Category("Test Category", "This is a test category.");
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            product = new Product(productName, "This is a test product.", null, 100u, 1, category);
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            product.Restock(1);
            await dbContext.SaveChangesAsync();
        }

        var existingOrders = await dbContext.Orders.Where(o => o.UserId == user.Id).ToListAsync();
        dbContext.Orders.RemoveRange(existingOrders);

        var existingTransactions = await dbContext.Transactions.Where(t => t.WalletId == user.Wallet.Id).ToListAsync();
        dbContext.Transactions.RemoveRange(existingTransactions);
        await dbContext.SaveChangesAsync();

        var client = httpClientFactory.CreateClient();
        var reqUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/orders";

        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            try
            {
                var result = await client.PostAsJsonAsync(reqUrl, new CreateOrderDto(user.Id, new List<CreateOrderItemDto> { new CreateOrderItemDto(product.Id, 1) }));
                var responseBody = await result.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {result.StatusCode} - Body: {responseBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });

        await Task.WhenAll(tasks);

        var updatedUser = await dbContext.Users.AsNoTracking().Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == user.Id);
        var orders = await dbContext.Orders.Select(o => new { o.Id, o.TotalPrice, o.UserId }).Where(o => o.UserId == user.Id).ToListAsync();
        var updatedProduct = await dbContext.Products.Select(p => new { p.Id, p.Name, p.Stock }).FirstOrDefaultAsync(p => p.Id == product.Id);

        return Results.Ok(new
        {
            User = new { Id = updatedUser!.Id, Name = updatedUser.Name, Balance = updatedUser.Wallet.Balance },
            Orders = orders,
            Product = updatedProduct,
        });
    }
}