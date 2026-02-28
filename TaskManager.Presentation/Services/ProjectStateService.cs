using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Entities;
namespace TaskManager.Presentation.Services
{
    public class ProjectStateService(IMemoryCache cache, IHttpContextAccessor accessor)
    {

        private readonly IMemoryCache _cache = cache;
        private string _userId =>
        accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";


        private string GetDetailsKey(Guid id) => $"{_userId}_project_details_{id}";
        private string GetTilesKey() => $"project_tiles_{_userId}";

        //----------- Getters ----------- 

        public ProjectDetailedViewDto? GetProjectDetails(Guid projectId)
        {
            Console.WriteLine("\n GETTING PROJECTS FROM IMEMORYCACHE \n");
            var original = _cache.Get<ProjectDetailedViewDto>(GetDetailsKey(projectId));
            return original is not null ? Clone(original) : null;
        }

        public List<ProjectTileDto>? GetUserProjectTiles()
        {
            Console.WriteLine("GETTING PROJECT TILES, LISTED USER: " + _userId);
            var original = _cache.Get<List<ProjectTileDto>>(GetTilesKey());
            return original is not null ? Clone(original) : null;

        }

        public TodoItemEntry? GetTodoItem(Guid projectId, Guid todoItemId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(GetDetailsKey(projectId))?
                .TodoItems
                .FirstOrDefault(t => t.Id == todoItemId);

            return original is not null ? Clone(original) : null;
        }

        public ProjectTileDto? GetProjectTile(Guid projectId)
        {
            var original = _cache
                .Get<List<ProjectTileDto>>(GetTilesKey())?
                .FirstOrDefault(p => p.Id == projectId);

            return original is not null ? Clone(original) : null;
        }

        public ProjectDetailsDto? GetProjectBasicDetails(Guid projectId)
        {

            var cachedProject = _cache.Get<ProjectDetailedViewDto>(GetDetailsKey(projectId));

            return cachedProject == null ? null : new ProjectDetailsDto
            {
                Id = cachedProject.Id,
                Title = cachedProject.Title,
                Description = cachedProject.Description,
                CreatedOn = cachedProject.CreatedOn
            };
        }

        public List<TodoItemEntry>? GetProjectTodoItems(Guid projectId)
        {
            var original = _cache.Get<ProjectDetailedViewDto>(GetDetailsKey(projectId));

            if (original is null) return null; 

            return Clone(original)?.TodoItems ?? null; 
        }


        //----------- Setters ----------- 

        public void SetProjectBasicDetails(ProjectDetailsDto details)
        {
            if (details is null) return;

            var project = GetProjectDetails(details.Id);

            var options = new MemoryCacheEntryOptions()
             .SetSlidingExpiration(TimeSpan.FromMinutes(20))
             .SetSize(1);

            if (project is null)
            {
                var projDetails = new ProjectDetailedViewDto
                {
                    Id = details.Id,
                    Title = details.Title,
                    Description = details.Description,
                    CreatedOn = details.CreatedOn,
                };

                _cache.Set(GetDetailsKey(projDetails.Id), projDetails, options);
            }

            else
            {
                //Id, Title, Description, CreatedOn
                project.Id = details.Id;
                project.Title = details.Title;
                project.Description = details.Description;
                project.CreatedOn = details.CreatedOn;

                _cache.Set(GetDetailsKey(project.Id), project, options);
            }

            return;
        }

        public void SetProjectTile(ProjectTileDto tile)
        {
            if (tile is null) return;

            var tiles = GetUserProjectTiles();

            if (tiles is null) return;

            var neededTile = tiles.FirstOrDefault(t => t.Id == tile.Id); 

            //Set new
            if (neededTile is null)
            {
                tiles.Add(tile);
            }

            //Overwrite
            else
            {
                neededTile = tile; 
            }

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            _cache.Set(GetTilesKey(), tiles, options);
        }

        public void SetProjectTiles(List<ProjectTileDto> projects)
        {
            Console.WriteLine("\n SETTING PROJECT TILES IN CACHE \n");
            if (projects == null) return;

            var options = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(20))
              .SetSize(1);

            _cache.Set(GetTilesKey(), projects, options);
        }

        public void SetAllProjectDetails(ProjectDetailedViewDto projectDetails)
        {
            if (projectDetails is null || string.IsNullOrEmpty(projectDetails.Title))
                return; 

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .SetSize(1);

            _cache.Set(GetDetailsKey(projectDetails.Id), projectDetails, options);
        }

        public void SetTodoItemInProject(Guid projectId, TodoItemEntry todoItem)
        {
            if (projectId == Guid.Empty ||  todoItem == null)
                return;


            var project = GetProjectDetails(projectId);

            if (project == null)
                return; 

            var existingIndex = project.TodoItems.FindIndex(t => t.Id == todoItem.Id);

            if (existingIndex != -1)
            {
                project.TodoItems[existingIndex] = todoItem;
            }
            else
            {
                project.TodoItems.Add(todoItem);
                project.TotalTodoItemCount++; 
            }


            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                .SetSize(1);

            _cache.Set(GetDetailsKey(projectId), project, options);
        }

        public void RemoveProject(Guid projectId)
        {
            if (projectId == Guid.Empty) return;

            _cache.Remove(GetDetailsKey(projectId));

            var tiles = GetUserProjectTiles();

            if (tiles is not null)
            {
                var tileToRemove = tiles.FirstOrDefault(p => p.Id == projectId);

                if (tileToRemove is not null)
                {
                    tiles.Remove(tileToRemove);
                    SetProjectTiles(tiles);
                }
            }
        }

        public void RemoveTodoItem(Guid projectId, Guid todoItemId)
        {

            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = GetProjectDetails(projectId);

            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);

            var isComplete = false;

            if (todoItem is not null)
            {
                //Handle Project Details
                project.TotalTodoItemCount--;

                if (todoItem.Status == Domain.Enums.Status.Complete)
                {
                    project.CompleteTodoItemCount--;
                    isComplete = true;
                }

                project.TodoItems.Remove(todoItem);
                SetAllProjectDetails(project);

                //Handle Project Tile
                var tiles = GetUserProjectTiles();

                if (tiles is null) return;

                var tileToUpdate = tiles.FirstOrDefault(p => p.Id == projectId);

                if (tileToUpdate is null) return; 

                tileToUpdate.TotalTodoItemCount--;

                if (isComplete)
                    tileToUpdate.CompleteTodoItemCount--;

                SetProjectTiles(tiles); 
            }
        }

        public void UpdateTodoItemStatus(Guid projectId, Guid todoItemId)
        {
            if (projectId == Guid.Empty || todoItemId == Guid.Empty) return;

            var project = GetProjectDetails(projectId);
            if (project is null || project.TodoItems is null) return;

            var todoItem = project.TodoItems.FirstOrDefault(t => t.Id == todoItemId);
            if (todoItem is null) return;

            //Discern which way we're flipping the status
            var wasComplete = todoItem.Status == Domain.Enums.Status.Complete;


            //If complete > Change to incomplete
            if (wasComplete)
            {
                project.CompleteTodoItemCount--;
                todoItem.Status = Domain.Enums.Status.Incomplete; 
            }

            else
            {
                project.CompleteTodoItemCount++;
                todoItem.Status = Domain.Enums.Status.Complete;
            }

            SetAllProjectDetails(project);
            
            //Handle Tile
            
            var tiles = GetUserProjectTiles();

            if (tiles is null) return;

            var tileToUpdate = tiles.FirstOrDefault(p => p.Id == projectId);

            if (tileToUpdate is null) return;

            if (wasComplete) { tileToUpdate.CompleteTodoItemCount--; }

            else { tileToUpdate.CompleteTodoItemCount++; }

            SetProjectTiles(tiles);
        }


        public ProjectDetailedViewDto? Clone(ProjectDetailedViewDto original)
        {
            if (original is null) return null;

            //Id, Title, Description, TotalTodoItemsCount, CompleteTodoItemsCount, Status, CreatedOn, TodoItems

            return new ProjectDetailedViewDto
            {
                Id = original.Id,
                Title = original.Title,
                Description = original.Description,
                TotalTodoItemCount = original.TotalTodoItemCount,
                CompleteTodoItemCount = original.CompleteTodoItemCount,
                Status = original.Status,
                CreatedOn = original.CreatedOn,
                TodoItems = original.TodoItems.Select(t => new TodoItemEntry
                {
                    //Id, AssigneeId, OwnerId, Title, Description, ProjectTitle, AssigneeName,
                    //OwnerName, Priority, DueDate, CreatedOn, Status

                    Id = t.Id,
                    AssigneeId = t.AssigneeId,
                    OwnerId = t.OwnerId,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectTitle = t.ProjectTitle,
                    AssigneeName = t.AssigneeName,
                    OwnerName = t.OwnerName,
                    DueDate = t.DueDate,
                    CreatedOn = t.CreatedOn,
                    Status = t.Status
                }).ToList()
            }; 
        }

        public ProjectTileDto? Clone(ProjectTileDto original)
        {
            if (original is null) return null;

            return new ProjectTileDto
            {
                Id = original.Id,
                OwnerId = original.OwnerId,
                Title = original.Title,
                Description = original.Description,
                TotalTodoItemCount = original.TotalTodoItemCount,
                CompleteTodoItemCount = original.CompleteTodoItemCount,
                CreatedOn = original.CreatedOn,
                Status = original.Status
            };
        }

        public List<ProjectTileDto>? Clone(List<ProjectTileDto> original)
        {
            if (original is null) return null;

            //Id, OwnerId, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, Status


            return original.Select(t => new ProjectTileDto 
            {

                Id = t.Id,
                OwnerId = t.OwnerId,
                Title = t.Title,
                Description = t.Description,
                TotalTodoItemCount = t.TotalTodoItemCount,
                CompleteTodoItemCount = t.CompleteTodoItemCount,
                CreatedOn = t.CreatedOn,
                Status = t.Status

            }).ToList();
        }

        public TodoItemEntry? Clone(TodoItemEntry original)
        {
            if (original is null) return null;

            return new TodoItemEntry
            {
                Id = original.Id,
                AssigneeId = original.AssigneeId,
                OwnerId = original.OwnerId,
                Title = original.Title,
                Description = original.Description,
                ProjectTitle = original.ProjectTitle,
                AssigneeName = original.AssigneeName,
                OwnerName = original.OwnerName,
                DueDate = original.DueDate,
                CreatedOn = original.CreatedOn,
                Status = original.Status
            };
        }

        public void ClearUserCache()
        {
            try
            {
                Console.WriteLine("\n \n CLEARING USER'S CACHE (HOPEFULLY UPON LOGOUT) \n\n");
                var tiles = GetUserProjectTiles();

                if (tiles is null || tiles.Count == 0) return;

                foreach (var tile in tiles)
                {
                    _cache.Remove(GetDetailsKey(tile.Id));
                }

                _cache.Remove(GetTilesKey());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis Cache Cleanup Error: {ex}");
            }
        }



        //public void SetProjectBasicDetails(ProjectDetailsDto basicDetails)
        //{
        //    if (basicDetails is null || string.IsNullOrWhiteSpace(basicDetails.Title))
        //        return;

        //    var details = GetProjectDetails(basicDetails.Id);

        //    if (details is not null)
        //    {
        //        details.Title = basicDetails.Title;
        //        details.Description = basicDetails.Description;
        //        details.CreatedOn = basicDetails.CreatedOn;
        //    }

        //    else
        //    {
        //        _projectDetailsCache[basicDetails.Id] = new ProjectDetailedViewDto
        //        {

        //            Id = basicDetails.Id,
        //            Title = basicDetails.Title,
        //            Description = basicDetails.Description,
        //            CreatedOn = basicDetails.CreatedOn,
        //        };
        //    }
        //}




        //public ProjectDetailedViewDto? GetProjectDetails(Guid projectId)
        //=> _projectDetailsCache.TryGetValue(projectId, out var details) ? details : null;


        //public TodoItemEntry? GetTodoItem(Guid projectId, Guid todoItemId)
        //{
        //    if (!_projectDetailsCache.Any() || projectId == Guid.Empty || todoItemId == Guid.Empty)
        //        return null;

        //    var project = GetProjectDetails(projectId);

        //    if (project is null)
        //        return null;

        //    var todoItem = project.TodoItems.Find(t => t.Id == todoItemId);


        //    if (todoItem is null)
        //        return null; 

        //    return todoItem; 
        //}

        //Getting tile view for all of the user's projects
        //public List<ProjectTileDto>? GetProjectTiles()
        //{

        //    var key = $"project_tiles_{_currUserId}";
        //    return _cache.Get<List<ProjectTileDto>>(key);

        //if (!_projectDetailsCache.Any())
        //    return new List<ProjectTileDto>();

        //Console.WriteLine("Retrieving Project Tiles From Cache");

        //var tiles = _projectDetailsCache.Values
        //    .Select(static p => new ProjectTileDto
        //    {
        //        Id = p.Id,
        //        Title = p.Title,
        //        Description = p.Description,
        //        TotalTodoItemCount = p.TotalTodoItemCount,
        //        CompleteTodoItemCount = p.CompleteTodoItemCount,
        //        CreatedOn = p.CreatedOn,
        //    }).ToList();

        //foreach (var tile in tiles)
        //{
        //    Console.WriteLine("Getting: ");
        //    Console.WriteLine(tile.Id + "\t" + tile.Title + "\t" + tile.Description + "\t" + tile.CompleteTodoItemCount + "/" + tile.TotalTodoItemCount);
        //}

        //return _projectDetailsCache.Values
        //    .Select(static p => new ProjectTileDto
        //    {
        //        Id = p.Id,
        //        Title = p.Title,
        //        Description = p.Description,
        //        TotalTodoItemCount = p.TotalTodoItemCount,
        //        CompleteTodoItemCount = p.CompleteTodoItemCount,
        //        CreatedOn = p.CreatedOn,
        //    }).ToList();
        //}

        //public ProjectTileDto? GetProjectTile(Guid id)
        //{
        //    var key = $"project_tiles_{_currUserId}";

        //    var tile = _cache.Get<ProjectTileDto>(key)?
        //        .FirstOrDefault(p => p.Id == id) ?? null;

        //    if (tile is not null)
        //    {
        //        return tile;
        //    }


        //    if (!_projectDetailsCache.Any())
        //        return null;

        //    var details = GetProjectDetails(id);
        //    if (details is null)
        //        return null;

        //    //Id, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, NeedsUpdate
        //    var newTile = new ProjectTileDto
        //    {
        //        Id = details.Id,
        //        Title = details.Title,
        //        Description = details.Description,
        //        TotalTodoItemCount = details.TotalTodoItemCount,
        //        CompleteTodoItemCount = details.CompleteTodoItemCount,
        //        CreatedOn = details.CreatedOn
        //    };

        //    return newTile; 
        //}

        //Retrieves the title/description of a single project
        //public ProjectDetailsDto? GetProjectBasicDetails(Guid id)
        //{
        //    if (id == Guid.Empty || !_projectDetailsCache.Any())
        //        return null;

        //    var details = GetProjectDetails(id);

        //    if (details is null || details.IsExpired)
        //        return null; 

        //    var detailsDto = new ProjectDetailsDto
        //    {
        //        Id = id,
        //        Title = details.Title,
        //        Description = details.Description,
        //        CreatedOn = details.CreatedOn,
        //    };

        //    return detailsDto;

        //}

        //public List<TodoItemEntry>? GetProjectTodoItems(Guid id)
        //{
        //    if (id == Guid.Empty || !_projectDetailsCache.Any())
        //        return null;


        //    var details = GetProjectDetails(id);

        //    if (details is null || details.IsExpired)
        //        return null;

        //    if (details is not null && details.TodoItems is not null)
        //    {
        //        return details.TodoItems.Select(t => new TodoItemEntry
        //        {
        //            Id = t.Id,
        //            Title = t.Title,
        //            ProjectTitle = t.ProjectTitle,
        //            AssigneeName = t.AssigneeName,
        //            Priority = t.Priority,
        //            DueDate = t.DueDate,
        //            CreatedOn = t.CreatedOn,
        //            Status = t.Status
        //        }).ToList();

        //    }

        //    return new List<TodoItemEntry>();
        //}


        //Setters

        //Sets a project's title/description/createdOn in cache
        //public void SetProjectBasicDetails(ProjectDetailsDto basicDetails)
        //{
        //    if (basicDetails is null || string.IsNullOrWhiteSpace(basicDetails.Title))
        //        return; 

        //    var details = GetProjectDetails(basicDetails.Id);

        //    if (details is not null)
        //    {
        //        details.Title = basicDetails.Title;
        //        details.Description = basicDetails.Description;
        //        details.CreatedOn = basicDetails.CreatedOn;
        //    }

        //    else
        //    {
        //        _projectDetailsCache[basicDetails.Id] = new ProjectDetailedViewDto
        //        {

        //            Id = basicDetails.Id,
        //            Title = basicDetails.Title,
        //            Description = basicDetails.Description,
        //            CreatedOn = basicDetails.CreatedOn,
        //        };
        //    }
        //}


        //Sets all of a user's projects' tiles in cache
        //public void SetProjectTiles(List<ProjectTileDto> projects)
        //{

        //    var key = $"project_tiles_{_userId}"; 

        //    var options = new MemoryCacheEntryOptions()
        //        .SetSlidingExpiration(TimeSpan.FromMinutes(20)) 
        //        .SetSize(1);


        //    _cache.Set(key, projects, options);

        //    foreach (var project in projects)
        //    {
        //        var existingDetails = GetProjectDetails(project.Id);

        //        if (existingDetails is null)
        //        {
        //            //Id, Title, Description, TotalTodoItemCount, CompleteTodoItemCount, CreatedOn, NeedsUpdate

        //            _projectDetailsCache.Add(project.Id, new ProjectDetailedViewDto
        //            {
        //                Id = project.Id,
        //                OwnerId = project.OwnerId,
        //                Title = project.Title,
        //                Description = project.Description,
        //                TotalTodoItemCount = project.TotalTodoItemCount,
        //                CompleteTodoItemCount = project.CompleteTodoItemCount,
        //                CreatedOn = project.CreatedOn,
        //                Status = project.Status,
        //                IsComplete = false
        //            }); 
        //        }

        //        if (existingDetails is not null)
        //        {
        //            _projectDetailsCache[project.Id] = new ProjectDetailedViewDto
        //            {
        //                Title = project.Title,
        //                Description = project.Description,
        //                TotalTodoItemCount = project.TotalTodoItemCount,
        //                CompleteTodoItemCount = project.CompleteTodoItemCount,
        //                CreatedOn = project.CreatedOn,
        //                IsComplete = false
        //            };
        //        }
        //    }
        //}

        //Sets a single project's tile in cache. MIGHT NEED TO REVISIT
        //public void SetProjectTileDetailsInCache(ProjectTileDto tile)
        //{
        //    if (tile is null)
        //        return;


        //    var details = GetProjectDetails(tile.Id);

        //    if (details is not null)
        //    {
        //        details.Id = tile.Id;
        //        details.Title = tile.Title;
        //        details.Description = tile.Description;
        //        details.TotalTodoItemCount = tile.TotalTodoItemCount;
        //        details.CompleteTodoItemCount = tile.CompleteTodoItemCount;
        //        details.CreatedOn = tile.CreatedOn;
        //    }

        //    else
        //    {
        //        _projectDetailsCache.Add(tile.Id, new ProjectDetailedViewDto
        //        {
        //            Id = tile.Id,
        //            Title = tile.Title,
        //            Description = tile.Description,
        //            TotalTodoItemCount = tile.TotalTodoItemCount,
        //            CompleteTodoItemCount = tile.CompleteTodoItemCount,
        //            CreatedOn = tile.CreatedOn,
        //        });
        //    }
        //}

        //Sets a whole view of a project in the cache
        //public void SetAllProjectDetails(ProjectDetailedViewDto projectDetails)
        //{
        //    if (projectDetails is null)
        //        return;

        //    var existingDetails = GetProjectDetails(projectDetails.Id);

        //    if (existingDetails is null)
        //    {
        //        projectDetails.IsComplete = true;
        //        _projectDetailsCache.Add(projectDetails.Id, projectDetails);
        //    }

        //    else
        //    {
        //        projectDetails.IsComplete = true;
        //        _projectDetailsCache[projectDetails.Id].TodoItems = projectDetails.TodoItems;
        //    }

        //}

        //Adds a task to a project in cache

        //public void AddTaskToProjectCache(Guid id, TodoItemEntry todoItem)
        //{
        //    if (id == Guid.Empty || todoItem == null)
        //        return;

        //    var project = GetProjectDetails(id);

        //    if (project is null)
        //        return;

        //    if (project.TodoItems is not null)
        //    {
        //        var todoItems = project.TodoItems.ToList(); 

        //        var existingIndex = project.TodoItems.FindIndex(t => t.Id == todoItem.Id);

        //        //If replacing an entry
        //        if (existingIndex != -1)
        //        {
        //            project.TodoItems.RemoveAt(existingIndex);
        //            project.TodoItems.Insert(existingIndex, todoItem);
        //            return;
        //        }

        //        project.TodoItems.Add(todoItem);
        //        project.TotalTodoItemCount++;
        //    }
        //}

        //Clears a project from the cache (after deletion)
        //public void RemoveFromCache(Guid projectId)
        //{
        //    _projectDetailsCache.Remove(projectId);
        //}

        //public void RemoveTodoItem(Guid projectId, Guid todoItemId)
        //{
        //    if (projectId == Guid.Empty || todoItemId == Guid.Empty)
        //        return;

        //    var existingDetails = GetProjectDetails(projectId);

        //    if (existingDetails is null || existingDetails.TodoItems is null)
        //        return;

        //    var todoItem = existingDetails.TodoItems.FirstOrDefault(t => t.Id == todoItemId);

        //    if (todoItem is not null)
        //    {
        //        if (todoItem.Status == Domain.Enums.Status.Complete)
        //        {
        //            existingDetails.TotalTodoItemCount--;
        //            existingDetails.CompleteTodoItemCount--;
        //        }

        //        else if (todoItem.Status == Domain.Enums.Status.Incomplete)
        //        {
        //            existingDetails.TotalTodoItemCount--;
        //        }

        //        existingDetails.TodoItems.Remove(todoItem);
        //    }

        //    return;
        //}


        //public void UpdateTodoItemStatus(Guid projectId, Guid todoItemId)
        //{
        //    if (projectId == Guid.Empty || todoItemId == Guid.Empty)
        //        return;

        //    var existingDetails = GetProjectDetails(projectId);

        //    if (existingDetails is null || existingDetails.TodoItems is null)
        //        return;

        //    var index = existingDetails.TodoItems.FindIndex(t => t.Id == todoItemId);

        //    if (index != -1)
        //    {
        //        var staleItem = existingDetails.TodoItems[index];

        //        if (staleItem is null)
        //            return;

        //        if(staleItem.Status == Domain.Enums.Status.Complete)
        //        {
        //            var updatedItem = new TodoItemEntry
        //            {
        //                Id = staleItem.Id,
        //                Title = staleItem.Title,
        //                ProjectTitle = staleItem.ProjectTitle,
        //                AssigneeName = staleItem.AssigneeName,
        //                OwnerName = staleItem.OwnerName,
        //                Priority = staleItem.Priority,
        //                DueDate = staleItem.DueDate,
        //                CreatedOn = staleItem.CreatedOn,
        //                Status = Domain.Enums.Status.Incomplete
        //            };

        //            existingDetails.CompleteTodoItemCount--;
        //            existingDetails.TodoItems[index] = updatedItem;

        //        }

        //        if (staleItem.Status == Domain.Enums.Status.Incomplete)
        //        {
        //            var updatedItem = new TodoItemEntry
        //            {
        //                Id = staleItem.Id,
        //                Title = staleItem.Title,
        //                ProjectTitle = staleItem.ProjectTitle,
        //                AssigneeName = staleItem.AssigneeName,
        //                OwnerName = staleItem.OwnerName,
        //                Priority = staleItem.Priority,
        //                DueDate = staleItem.DueDate,
        //                CreatedOn = staleItem.CreatedOn,
        //                Status = Domain.Enums.Status.Complete
        //            }; 

        //            existingDetails.CompleteTodoItemCount++;
        //            existingDetails.TodoItems[index] = updatedItem;
        //        }

        //        _projectDetailsCache[projectId] = existingDetails;

        //        return;

        //    }
        //}
    }
}
