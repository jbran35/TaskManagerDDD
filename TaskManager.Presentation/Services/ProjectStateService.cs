using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
namespace TaskManager.Presentation.Services
{
    public class ProjectStateService
    {
        private Dictionary<Guid, ProjectDetailedViewDto> _projectDetailsCache = new Dictionary<Guid, ProjectDetailedViewDto>();

        //Getters

        //Getting a whole view of a single project
        public ProjectDetailedViewDto? GetProjectDetails(Guid projectId)
        => _projectDetailsCache.TryGetValue(projectId, out var details) ? details : null;


        public TodoItemEntry? GetTodoItem(Guid projectId, Guid todoItemId)
        {
            if (!_projectDetailsCache.Any() || projectId == Guid.Empty || todoItemId == Guid.Empty)
                return null;

            var project = GetProjectDetails(projectId);

            if (project is null)
                return null;

            var todoItem = project.TodoItems.Find(t => t.Id == todoItemId);


            if (todoItem is null)
                return null; 
                
            return todoItem; 
        }

        //Getting tile view for all of the user's projects
        public List<ProjectTileDto>? GetProjectTiles()
        {
            if (!_projectDetailsCache.Any())
                return new List<ProjectTileDto>();

            Console.WriteLine("Retrieving Project Tiles From Cache");

            var tiles = _projectDetailsCache.Values
                .Select(static p => new ProjectTileDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    TotalTodoItemCount = p.TotalTodoItemCount,
                    CompleteTodoItemCount = p.CompleteTodoItemCount,
                    CreatedOn = p.CreatedOn,
                }).ToList();

            foreach (var tile in tiles)
            {
                Console.WriteLine("Getting: ");
                Console.WriteLine(tile.Id + "\t" + tile.Title + "\t" + tile.Description + "\t" + tile.CompleteTodoItemCount + "/" + tile.TotalTodoItemCount);
            }

            return _projectDetailsCache.Values
                .Select(static p => new ProjectTileDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    TotalTodoItemCount = p.TotalTodoItemCount,
                    CompleteTodoItemCount = p.CompleteTodoItemCount,
                    CreatedOn = p.CreatedOn,
                }).ToList();
        }

        public ProjectTileDto? GetProjectTile(Guid id)
        {
            if (!_projectDetailsCache.Any())
                return null;

            var details = GetProjectDetails(id);
            if (details is null)
                return null;

            //Id, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, NeedsUpdate
            var newTile = new ProjectTileDto
            {
                Id = details.Id,
                Title = details.Title,
                Description = details.Description,
                TotalTodoItemCount = details.TotalTodoItemCount,
                CompleteTodoItemCount = details.CompleteTodoItemCount,
                CreatedOn = details.CreatedOn
            };

            return newTile; 
        }

        //Retrieves the title/description of a single project
        public ProjectDetailsDto? GetProjectBasicDetailsFromCache(Guid id)
        {
            if (id == Guid.Empty || !_projectDetailsCache.Any())
                return null;

            var details = GetProjectDetails(id);

            if (details is null || details.IsExpired)
                return null; 

            var detailsDto = new ProjectDetailsDto
            {
                Id = id,
                Title = details.Title,
                Description = details.Description,
                CreatedOn = details.CreatedOn,
            };

            return detailsDto;

        }

        public List<TodoItemEntry>? GetProjectTodoItemsFromCache(Guid id)
        {
            if (id == Guid.Empty || !_projectDetailsCache.Any())
                return null;

          
            var details = GetProjectDetails(id);

            if (details is null || details.IsExpired)
                return null;
            
            if (details is not null && details.TodoItems is not null)
            {
                return details.TodoItems.Select(t => new TodoItemEntry
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectTitle = t.ProjectTitle,
                    AssigneeName = t.AssigneeName,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedOn = t.CreatedOn,
                    Status = t.Status
                }).ToList();
                 
            }

            return new List<TodoItemEntry>();
        }


        //Setters

        //Sets a project's title/description/createdOn in cache
        public void SetProjectBasicDetailsInCache(ProjectDetailsDto basicDetails)
        {
            if (basicDetails is null || string.IsNullOrWhiteSpace(basicDetails.Title))
                return; 

            var details = GetProjectDetails(basicDetails.Id);

            if (details is not null)
            {
                details.Title = basicDetails.Title;
                details.Description = basicDetails.Description;
                details.CreatedOn = basicDetails.CreatedOn;
            }

            else
            {
                _projectDetailsCache[basicDetails.Id] = new ProjectDetailedViewDto
                {
                    
                    Id = basicDetails.Id,
                    Title = basicDetails.Title,
                    Description = basicDetails.Description,
                    CreatedOn = basicDetails.CreatedOn,
                };
            }
        }


        //Sets all of a user's projects' tiles in cache
        public void SetProjectTilesInCache(List<ProjectTileDto> projects)
        {
            foreach (var project in projects)
            {
                var existingDetails = GetProjectDetails(project.Id);

                if (existingDetails is null)
                {
                    //Id, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, NeedsUpdate

                    _projectDetailsCache.Add(project.Id, new ProjectDetailedViewDto
                    {
                        Id = project.Id,
                        OwnerId = project.OwnerId,
                        Title = project.Title,
                        Description = project.Description,
                        TotalTodoItemCount = project.TotalTodoItemCount,
                        CompleteTodoItemCount = project.CompleteTodoItemCount,
                        CreatedOn = project.CreatedOn,
                        Status = project.Status,
                        IsComplete = false
                    }); 
                }

                if (existingDetails is not null)
                {
                    _projectDetailsCache[project.Id] = new ProjectDetailedViewDto
                    {
                        Title = project.Title,
                        Description = project.Description,
                        TotalTodoItemCount = project.TotalTodoItemCount,
                        CompleteTodoItemCount = project.CompleteTodoItemCount,
                        CreatedOn = project.CreatedOn,
                        IsComplete = false
                    };
                }
            }
        }

        //Sets a single project's tile in cache.
        public void SetProjectTileDetailsInCache(ProjectTileDto tile)
        {
            if (tile is null)
                return;


            var details = GetProjectDetails(tile.Id);

            if (details is not null)
            {
                details.Id = tile.Id;
                details.Title = tile.Title;
                details.Description = tile.Description;
                details.TotalTodoItemCount = tile.TotalTodoItemCount;
                details.CompleteTodoItemCount = tile.CompleteTodoItemCount;
                details.CreatedOn = tile.CreatedOn;
            }

            else
            {
                _projectDetailsCache.Add(tile.Id, new ProjectDetailedViewDto
                {
                    Id = tile.Id,
                    Title = tile.Title,
                    Description = tile.Description,
                    TotalTodoItemCount = tile.TotalTodoItemCount,
                    CompleteTodoItemCount = tile.CompleteTodoItemCount,
                    CreatedOn = tile.CreatedOn,
                });
            }
        }

        //Sets a whole view of a project in the cache
        public void SetAllProjectDetailsInCache(ProjectDetailedViewDto projectDetails)
        {
            if (projectDetails is null)
                return;

            var existingDetails = GetProjectDetails(projectDetails.Id);

            if (existingDetails is null)
            {
                projectDetails.IsComplete = true;
                _projectDetailsCache.Add(projectDetails.Id, projectDetails);
            }

            else
            {
                projectDetails.IsComplete = true;
                _projectDetailsCache[projectDetails.Id].TodoItems = projectDetails.TodoItems;
            }
               
        }

        //Adds a task to a project in cache

        public void AddTaskToProjectCache(Guid id, TodoItemEntry todoItem)
        {
            if (id == Guid.Empty || todoItem == null)
                return;

            var project = GetProjectDetails(id);

            if (project is null)
                return;

            if (project.TodoItems is not null)
            {
                var todoItems = project.TodoItems.ToList(); 

                var existingIndex = project.TodoItems.FindIndex(t => t.Id == todoItem.Id);

                //If replacing an entry
                if (existingIndex != -1)
                {
                    project.TodoItems.RemoveAt(existingIndex);
                    project.TodoItems.Insert(existingIndex, todoItem);
                    return;
                }

                project.TodoItems.Add(todoItem);
                project.TotalTodoItemCount++;
            }
        }

        //Clears a project from the cache (after deletion)
        public void RemoveFromCache(Guid projectId)
        {
            _projectDetailsCache.Remove(projectId);
        }

        public void RemoveTodoItemFromCache(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty)
                return;

            var existingDetails = GetProjectDetails(projectId);

            if (existingDetails is null || existingDetails.TodoItems is null)
                return;

            var todoItem = existingDetails.TodoItems.FirstOrDefault(t => t.Id == todoItemId);

            if (todoItem is not null)
            {
                if (todoItem.Status == Domain.Enums.Status.Complete)
                {
                    existingDetails.TotalTodoItemCount--;
                    existingDetails.CompleteTodoItemCount--;
                }

                else if (todoItem.Status == Domain.Enums.Status.Incomplete)
                {
                    existingDetails.TotalTodoItemCount--;
                }

                existingDetails.TodoItems.Remove(todoItem);
            }
                
            return;
        }


        public void UpdateTodoItemStatus(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty)
                return;

            var existingDetails = GetProjectDetails(projectId);

            if (existingDetails is null || existingDetails.TodoItems is null)
                return;

            var index = existingDetails.TodoItems.FindIndex(t => t.Id == todoItemId);

            if (index != -1)
            {
                var staleItem = existingDetails.TodoItems[index];

                if (staleItem is null)
                    return;

                if(staleItem.Status == Domain.Enums.Status.Complete)
                {
                    var updatedItem = new TodoItemEntry
                    {
                        Id = staleItem.Id,
                        Title = staleItem.Title,
                        ProjectTitle = staleItem.ProjectTitle,
                        AssigneeName = staleItem.AssigneeName,
                        OwnerName = staleItem.OwnerName,
                        Priority = staleItem.Priority,
                        DueDate = staleItem.DueDate,
                        CreatedOn = staleItem.CreatedOn,
                        Status = Domain.Enums.Status.Incomplete
                    };

                    existingDetails.CompleteTodoItemCount--;
                    existingDetails.TodoItems[index] = updatedItem;

                }

                if (staleItem.Status == Domain.Enums.Status.Incomplete)
                {
                    var updatedItem = new TodoItemEntry
                    {
                        Id = staleItem.Id,
                        Title = staleItem.Title,
                        ProjectTitle = staleItem.ProjectTitle,
                        AssigneeName = staleItem.AssigneeName,
                        OwnerName = staleItem.OwnerName,
                        Priority = staleItem.Priority,
                        DueDate = staleItem.DueDate,
                        CreatedOn = staleItem.CreatedOn,
                        Status = Domain.Enums.Status.Complete
                    }; 
                    
                    existingDetails.CompleteTodoItemCount++;
                    existingDetails.TodoItems[index] = updatedItem;
                }

                _projectDetailsCache[projectId] = existingDetails;

                return;

            }
        }
    }
}
