using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.Queries;
using TaskManager.Application.TodoItems.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.QueryHandlers
{
    public class GetProjectDetailedViewQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache) : IRequestHandler<GetProjectDetailedViewQuery, Result<ProjectDetailedViewDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache; 

        public async Task<Result<ProjectDetailedViewDto>> Handle(GetProjectDetailedViewQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.ProjectId == Guid.Empty || request.UserId == Guid.Empty)
                return Result<ProjectDetailedViewDto>.Failure("Invalid Request");


            //Check Cache
            string key = $"projects_{request.UserId}";

            //var cachedProjects = await _cache.GetStringAsync(key, cancellationToken);

            //if (!string.IsNullOrEmpty(cachedProjects))
            //{
            //    Console.WriteLine(" \n PULLING FROM REDIS CACHE");
            //    var projects = JsonSerializer.Deserialize<List<ProjectTileDto>>(cachedProjects);
            //    return Result<List<ProjectTileDto>>.Success(projects!);
            //}


            //Validate project
            var project = await _unitOfWork.ProjectRepository.GetProjectDetailedViewAsync(request.ProjectId, cancellationToken);

            if (project is null || project.OwnerId != request.UserId)
                return Result<ProjectDetailedViewDto>.Failure("Project Not Found");

            //Map project to DTO & Return
            var projectDetailedViewDto = new ProjectDetailedViewDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Status = project.Status,
                CreatedOn = project.CreatedOn,
                TotalTodoItemCount = project.TotalTodoItemCount,
                CompleteTodoItemCount = project.CompleteTodoItemCount,

                TodoItems = project.TodoItems.Select(static t => new TodoItemEntry
                {
                    Id = t.Id,
                    OwnerId = t.OwnerId,
                    AssigneeId = t.AssigneeId,
                    Title = t.Title,
                    Description = t.Description,
                    ProjectTitle = t.ProjectTitle,
                    AssigneeName = t.AssigneeName,
                    OwnerName = t.OwnerName,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedOn = t.CreatedOn,
                    Status = t.Status
                }
                ).ToList()
            };

            return Result<ProjectDetailedViewDto>.Success(projectDetailedViewDto);
        }
    }
}
