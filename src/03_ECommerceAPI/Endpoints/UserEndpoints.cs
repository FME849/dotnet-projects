using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _03_EcommerceAPI.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", CreateUser);
        app.MapGet("/users/{id}", GetUser);
        app.MapPatch("/wallets/{id}/deposit", DepositWallet);
    }

    private static async Task<IResult> CreateUser(CreateUserDto createUserDto, EcommerceDbContext dbContext)
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
    }

    private static async Task<IResult> GetUser(Guid id, EcommerceDbContext dbContext)
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
    }

    private static async Task<IResult> DepositWallet(Guid id, DepositDtos depositDtos, EcommerceDbContext dbContext)
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
    }
}
