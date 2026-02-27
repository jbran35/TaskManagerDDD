using MediatR;
using TaskManager.Application.Projects.DTOs.Responses;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Projects.Commands
{
    public record CreateProjectCommand(
        Guid UserId,
        string? Title,
        string? Description) : IRequest<Result<CreateProjectResponse>>;
    
}
