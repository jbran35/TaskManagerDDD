using TaskManager.Application.TodoItems.DTOs;

namespace TaskManager.Presentation.Services
{
    public class AssignedTodoItemsStateService
    {
        private List<TodoItemEntry> _assignedTodoItemsCache = new List<TodoItemEntry>(); 

        public List<TodoItemEntry>? GetTodoItems()
        {
            return _assignedTodoItemsCache;
        }

        public void SetTodoItemsAsync(List<TodoItemEntry> todoItems)
        {
            _assignedTodoItemsCache = todoItems;
        }

        //public void ClearAssigneeFromTodoItems(Guid id)
        //{
        //    foreach(var todoItem in _projectCache)
        //    {
        //        var tasksToUpdate = todoItem.Value.Where(t => t.AssigneeId == id).ToList();

        //        foreach(var task in tasksToUpdate)
        //        {
        //            var updatedTask = new TodoItemListEntryDto
        //            {
        //                //Includes: ProjectId, ProjectName, TodoItemId, Title, AssigneeId, AssigneeName, DueDate, Status, Priority

        //                ProjectId = task.ProjectId,
        //                ProjectName = task.ProjectName,
        //                TodoItemId = task.TodoItemId,
        //                Title = task.Title,
        //                AssigneeId = Guid.Empty,
        //                AssigneeName = string.Empty, 
        //                DueDate = task.DueDate, 
        //                Status = task.Status, 
        //                Priority = task.Priority
        //            };

        //            MarkExpiredAsync(task.ProjectId);



        //        }

        //    }
        //}

        //public void MarkExpiredAsync(Guid projectId)
        //{
        //    _assignedTodoItemsCache.Remove(projectId);
        //}
    }
}
