using System.Diagnostics;
using _02_TaskManagerAPI.Services;
using _02_TaskManagerAPI.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ITodoService, TodoService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    Stopwatch sw = Stopwatch.StartNew();
    await next(context);
    sw.Stop();

    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path} responded {context.Response.StatusCode} in {sw.ElapsedMilliseconds}ms");
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/openapi"))
    {
        await next(context);
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey) || apiKey != "my-secret-api-key")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized request. Please provide a valid API key.");
    }
    else
    {
        await next(context);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Get all todos
app.MapGet("/api/todos", (ITodoService todoService) =>
{
    var todos = todoService.GetTodoList();
    return Results.Ok(todos);
});

// Create a new todo
app.MapPost("/api/todos", (CreateTodoDto createTodoDto, ITodoService todoService) =>
{
    var newTodo = todoService.AddTodo(createTodoDto.Title, createTodoDto.Description);
    return Results.Created("", newTodo);
});

// Update an existing todo
app.MapPut("/api/todos/{id:guid}", (Guid id, UpdateTodoDto updateTodoDto, ITodoService todoService) =>
{
    var success = todoService.UpdateTodo(id, updateTodoDto.Title, updateTodoDto.Description);
    if (success)
    {
        return Results.NoContent();
    }
    else
    {
        return Results.NotFound();
    }
});


// Delete a todo
app.MapDelete("/api/todos/{id:guid}", (Guid id, ITodoService todoService) =>
{
    var success = todoService.DeleteTodo(id);
    if (success)
    {
        return Results.NoContent();
    }
    else
    {
        return Results.NotFound();
    }
});

// Complete a todo
app.MapPatch("/api/todos/{id:guid}/complete", (Guid id, CompleteTodoDto completeTodoDto, ITodoService todoService) =>
{
    var success = todoService.CompleteTodo(id, completeTodoDto.IsCompleted);
    if (success)
    {
        return Results.NoContent();
    }
    else
    {
        return Results.NotFound();
    }
});

app.Run();
