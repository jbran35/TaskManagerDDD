using MediatR;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.QueryHandlers
{
    public class GetProjectsDetailsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        public async Task<Result<ProjectDetailsDto>> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if(request is null || request.ProjectId == Guid.Empty || request.UserId == Guid.Empty)
                return Result<ProjectDetailsDto>.Failure("Invalid Request");


            //Validate Project
            var project = await _unitOfWork.ProjectRepository.GetProjectWithoutTasksAsync(request.ProjectId, cancellationToken);

            if (project is null || project.OwnerId != request.UserId)
                return Result<ProjectDetailsDto>.Failure("Project Not Found");

            //Map to DTO and return

            var detailsDto = new ProjectDetailsDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedOn = project.CreatedOn
            }; 

            return Result<ProjectDetailsDto>.Success(detailsDto);

        }
    }
}
