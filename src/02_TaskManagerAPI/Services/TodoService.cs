using _02_TaskManagerAPI.Models;

namespace _02_TaskManagerAPI.Services;

public interface ITodoService
{
    public IReadOnlyList<Todo> GetTodoList();
    public Todo AddTodo(string title, string? description);
    public bool UpdateTodo(Guid id, string? title, string? description);
    public bool DeleteTodo(Guid id);
    public bool CompleteTodo(Guid id, bool isCompleted);
}

public class TodoService : ITodoService
{
    private List<Todo> _todoList = new();
    private readonly Lock _lock = new();

    public IReadOnlyList<Todo> GetTodoList()
    {
        lock(_lock)
        {
            return _todoList.ToList();
        }
    }

    public Todo AddTodo(string title, string? description)
    {
        lock(_lock)
        {
            Todo todo = new(title, description);
            _todoList.Add(todo);
            return todo;
        }
    }

    public bool UpdateTodo(Guid id, string? title, string? description)
    {
        lock(_lock)
        {
            Todo? todo = GetTodoById(id);
            if (todo == null)
            {
                return false;
            }
            todo.Update(title, description);
            return true;
        }
    }

    public bool DeleteTodo(Guid id)
    {
        lock(_lock)
        {
            Todo? todo = GetTodoById(id);
            if (todo == null)
            {
                return false;
            }
            _todoList.Remove(todo);
            return true;
        }
    }

    public bool CompleteTodo(Guid id, bool isCompleted)
    {
        lock(_lock)
        {
            Todo? todo = GetTodoById(id);
            if (todo == null)
            {
                return false;
            }
            todo.Complete(isCompleted);
            return true;
        }
    }

    private Todo? GetTodoById(Guid id)
    {
        return _todoList.FirstOrDefault(todo => todo.Id == id);
    }
}
