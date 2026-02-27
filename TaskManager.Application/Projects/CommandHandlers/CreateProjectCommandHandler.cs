using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class CreateProjectCommandHandler(
        IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<CreateProjectCommand, Result<CreateProjectResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;

        public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request is null || request.UserId == Guid.Empty)
                return Result<CreateProjectResponse>.Failure("Invalid request.");

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<CreateProjectResponse>.Failure("User not found.");

            // Validate and create value objects
            var projectTitleResult = Title.Create(request.Title);

            if (projectTitleResult.IsFailure)
                return Result<CreateProjectResponse>.Failure(projectTitleResult.ErrorMessage ?? "Invalid project title.");


            var projectDescriptionResult = Description.Create(request.Description);

            if (projectDescriptionResult.IsFailure)
                return Result<CreateProjectResponse>.Failure(projectDescriptionResult.ErrorMessage ?? "Invalid project description.");


            // Create the project entity
            var projectResult = Project.Create(projectTitleResult.Value, projectDescriptionResult.Value, request.UserId);

            if (projectResult.IsFailure)
                return Result<CreateProjectResponse>.Failure(projectResult.ErrorMessage ?? "Failed to create project.");

            var projectTileDto = new ProjectTileDto
            {
                Id = projectResult.Value.Id,
                Title = projectResult.Value.Title,
                Description = projectResult.Value.Description,
                TotalTodoItemCount = projectResult.Value.TodoItems.Count,
                CompleteTodoItemCount = projectResult.Value.TodoItems.Where(t => t.Status == Domain.Enums.Status.Complete).Count(),
                CreatedOn = projectResult.Value.CreatedOn,
            };

            // Save the project to the database
            try
            {
                _unitOfWork.ProjectRepository.Add(projectResult.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                return Result<CreateProjectResponse>.Success(new CreateProjectResponse
                {
                    CreatedProject = projectTileDto,
                }, "Project Created Successfully"); 
                   
            }

            catch (Exception)
            {   
                return Result<CreateProjectResponse>.Failure("An error occurred while saving the project.");
            }

        }
    }
}
