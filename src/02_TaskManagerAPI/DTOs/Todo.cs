namespace _02_TaskManagerAPI.DTOs;

public class CreateTodoDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
}

public class UpdateTodoDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
}

public class CompleteTodoDto
{
    public required bool IsCompleted { get; set; }
}