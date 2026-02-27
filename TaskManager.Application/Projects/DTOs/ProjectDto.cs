using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.DTOs
{
    public record ProjectDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; } = string.Empty;
        public int TotalTodoItemCount { get; set; } = 0;
        public int CompleteTodoItemCount { get; set; } = 0;
        public DateTime CreatedOn { get; set; }
        public Status Status { get; set; } = Status.Incomplete;


        public List<TodoItemEntry>? TodoItems { get; set; }
    }
}
