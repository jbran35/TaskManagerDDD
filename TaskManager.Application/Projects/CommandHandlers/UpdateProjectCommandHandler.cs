using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.CommandHandlers
{
    public class UpdateProjectCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : IRequestHandler<UpdateProjectCommand, Result<UpdateProjectResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<User> _userManager = userManager;
        public async Task<Result<UpdateProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request is null || request.UserId == Guid.Empty || request.ProjectId == Guid.Empty)
                return Result<UpdateProjectResponse>.Failure("Invalid request.");

            // Check if the user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return Result<UpdateProjectResponse>.Failure("User not found.");

            // Check if the project exists
            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            if (project is null)
                return Result<UpdateProjectResponse>.Failure("Project not found.");

            // Check if the user is the owner of the project
            if (project.OwnerId != request.UserId)
                return Result<UpdateProjectResponse>.Failure("Unauthorized: You do not have permission to update this project.");

            if(request.NewTitle is not null && request.NewTitle != project.Title)
            {
                var updateTitleResult = project.UpdateTitle(request.NewTitle); 
                if (updateTitleResult.IsFailure)
                    return Result<UpdateProjectResponse>.Failure(updateTitleResult.ErrorMessage ?? "Failed to update project Title.");
            }

            if (request.NewDescription is not null && request.NewDescription != project.Description)
            {
                // Update the project description
                var updateDescriptionResult = project.UpdateDescription(request.NewDescription);
                if (updateDescriptionResult.IsFailure)
                    return Result<UpdateProjectResponse>.Failure(updateDescriptionResult.ErrorMessage ?? "Failed to update project description.");
            }

            // Save changes to the database
            try
            {
                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var updatedDetails = new ProjectDetailsDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    CreatedOn = project.CreatedOn

                };

                Console.WriteLine("In UpdateProj Handler - New Description: " + updatedDetails.Description ?? "Is Null");

                var response = new UpdateProjectResponse
                {
                    ProjectDetails = updatedDetails
                };

                return Result<UpdateProjectResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<UpdateProjectResponse>.Failure($"An error occurred while updating the project description.");
            }
        }
    }
}
