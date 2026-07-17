using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _03_EcommerceAPI.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", CreateCategory);
        app.MapGet("/categories", GetCategories);
    }

    private static async Task<IResult> CreateCategory(CreateCategoryDto createCategoryDto, EcommerceDbContext dbContext)
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
    }

    private static async Task<IResult> GetCategories(EcommerceDbContext dbContext)
    {
        var categories = await dbContext.Categories.ToListAsync();
        return Results.Ok(categories);
    }
}