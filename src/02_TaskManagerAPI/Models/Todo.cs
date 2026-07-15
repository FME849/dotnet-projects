namespace _02_TaskManagerAPI.Models;

public class Todo
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Todo(string title, string? description)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Todo(Guid id, string title, bool isCompleted, DateTime createdAt, string? description)
    {
        Id = id;
        Title = title;
        IsCompleted = isCompleted;
        CreatedAt = createdAt;
        Description = description;
    }

    public void Update(string? title, string? description)
    {
        if (!string.IsNullOrEmpty(title))
        {
            Title = title;
        }

        if (!string.IsNullOrEmpty(description))
        {
            Description = description;
        }
        else if (description == string.Empty)
        {
            Description = null;
        }
    }

    public void Complete(bool isCompleted)
    {
        IsCompleted = isCompleted;
    }
}