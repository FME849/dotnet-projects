using _03_EcommerceAPI.Data;
using _03_EcommerceAPI.DTOs;
using _03_EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace _03_EcommerceAPI.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products", CreateProduct);
        app.MapGet("/products", GetProducts);
    }

    private static async Task<IResult> CreateProduct(CreateProductDto createProductDto, EcommerceDbContext dbContext)
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
            return Results.BadRequest("Failed to create product");
        }
    }

    private static async Task<IResult> GetProducts(EcommerceDbContext dbContext)
    {
        var products = await dbContext.Products.Include(p => p.Category).Select(p => new { p.Id, p.Name, p.Description, p.ImageUrl, p.Price, p.Stock, CategoryName = p.Category.Name }).ToListAsync();
        return Results.Ok(products);
    }
}