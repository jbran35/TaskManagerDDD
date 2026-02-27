using MediatR;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record UpdateProjectCommand(
       Guid UserId,
       Guid ProjectId,
       string? NewTitle,
       string? NewDescription
    ) : IRequest<Result<UpdateProjectResponse>>;
 
}
