using System.Diagnostics;
using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EcommerceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next(context);
    sw.Stop();
    Console.WriteLine($"Request took {sw.ElapsedMilliseconds} ms");
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/categories", async (CreateCategoryDto createCategoryDto, EcommerceDbContext dbContext) =>
{
    var category = new Category(createCategoryDto.Name, createCategoryDto.Description);
    dbContext.Categories.Add(category);
    var result = await dbContext.SaveChangesAsync();

    if (result > 0)
    {
        return Results.Created($"/categories/{category.Id}", category);
    }
    else
    {
        return Results.BadRequest("Failed to create category");
    }
});

app.MapGet("/categories", async (EcommerceDbContext dbContext) =>
{
    var categories = await dbContext.Categories.ToListAsync();
    return Results.Ok(categories);
});

app.MapPost("/products", async (CreateProductDto createProductDto, EcommerceDbContext dbContext) =>
{
    var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == createProductDto.CategoryId);

    if (category == null)
    {
        return Results.NotFound($"Category not found for id: {createProductDto.CategoryId}");
    }

    var product = new Product(createProductDto.Name, createProductDto.Description, createProductDto.ImageUrl, createProductDto.Price, createProductDto.Stock, category);
    dbContext.Products.Add(product);
    var result = await dbContext.SaveChangesAsync();

    if (result > 0)
    {
        return Results.Created($"/products/{product.Id}", product);
    }
    else
    {
        return Results.BadRequest("Failed to create product.");
    }
});

app.MapGet("/products", async (EcommerceDbContext dbContext) =>
{
    var products = await dbContext.Products.Include(p => p.Category).ToListAsync();
    return Results.Ok(products);
});

app.MapPost("/users", async (CreateUserDto createUserDto, EcommerceDbContext dbContext) =>
{
    var isExist = await dbContext.Users.AnyAsync(u => u.Email == createUserDto.Email);
    if (isExist)
    {
        return Results.BadRequest($"User with email {createUserDto.Email} has already existed.");
    }

    var user = new User(createUserDto.Name, createUserDto.Email, createUserDto.Password); // Should hash the password 1st, but leaving it as is to keep it simple
    dbContext.Users.Add(user);
    var result = await dbContext.SaveChangesAsync();

    if (result > 0)
    {
        return Results.Created($"/users/{user.Id}", new ResponseUserDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Wallet.Balance));
    }
    else
    {
        return Results.BadRequest("Failed to create user.");
    }
});

app.MapGet("/users/{id}", async (Guid id, EcommerceDbContext dbContext) =>
{
    var user = await dbContext.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == id);
    if (user != null)
    {
        return Results.Ok(new ResponseUserDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Wallet.Balance));
    }
    else
    {
        return Results.NotFound($"Could not found user with id {id}.");
    }
});

app.MapPatch("/wallets/{id}/deposit", async (Guid id, DepositDtos depositDtos, EcommerceDbContext dbContext) =>
{
    var wallet = await dbContext.Wallets.Include(w => w.User).FirstOrDefaultAsync(w => w.Id == id && w.UserId == depositDtos.UserId);
    if (wallet != null)
    {
        wallet.Deposit(depositDtos.Amount);
        var transaction = new Transaction(wallet, depositDtos.Amount, TransactionType.Deposit);
        dbContext.Transactions.Add(transaction);
        var result = await dbContext.SaveChangesAsync();
        if (result > 0)
        {
            return Results.NoContent();
        }
        else
        {
            return Results.BadRequest($"Failed to deposit wallet with id {id}");
        }
    }
    else
    {
        return Results.NotFound($"Could not found wallet with id {id} or user id {depositDtos.UserId}.");
    }
});

app.MapPost("/orders", async (CreateOrderDto createOrderDto, EcommerceDbContext dbContext) =>
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

        products.ForEach(product =>
        {
            var quantity = createOrderDto.Items.FirstOrDefault(dto => dto.ProductId == product.Id)!.Quantity;
            product.ReduceStock(quantity);
            order.AddItem(product, quantity);
        });

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
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Results.BadRequest(ex.Message);
    }

});

app.MapPost("/test-concurrency", async (HttpContext httpContext, IHttpClientFactory httpClientFactory, EcommerceDbContext dbContext) =>
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

    var updatedUser = await dbContext.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == user.Id);
    var orders = await dbContext.Orders.Select(o => new { o.Id, o.TotalPrice, o.UserId }).Where(o => o.UserId == user.Id).ToListAsync();
    var updatedProduct = await dbContext.Products.Select(p => new { p.Id, p.Name, p.Stock }).FirstOrDefaultAsync(p => p.Id == product.Id);

    return Results.Ok(new
    {
        User = new { Id = updatedUser!.Id, Name = updatedUser.Name, Balance = updatedUser.Wallet.Balance },
        Orders = orders,
        Product = updatedProduct,
    });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
