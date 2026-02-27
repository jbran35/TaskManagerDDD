using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TaskManager.Application.Projects.DTOs;
using TaskManager.Application.Projects.Queries;
using TaskManager.Domain.Common;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Projects.QueryHandlers
{
    public class GetUserProjectsQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache) : IRequestHandler<GetUserProjectsQuery, Result<List<ProjectTileDto>>>
    {
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache; 
        public async Task<Result<List<ProjectTileDto>>> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
        {
            //Validate Request
            if (request is null || request.UserId == Guid.Empty)
                return Result<List<ProjectTileDto>>.Failure("Invalid request");

            //Check Cache
            string key = $"projects_{request.UserId}";

            var cachedProjects = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(cachedProjects))
            {
                Console.WriteLine(" \n PULLING FROM REDIS CACHE");
                var projects = JsonSerializer.Deserialize<List<ProjectTileDto>>(cachedProjects);
                return Result<List<ProjectTileDto>>.Success(projects!);
            }


            //Validate projects list
            var projectTiles = await unitOfWork.ProjectRepository.GetAllProjectsByOwnerIdAsync(request.UserId, cancellationToken);

            if (projectTiles is null)
                return Result<List<ProjectTileDto>>.Failure("Issue Loading Projects");


            //Save to cache
            Console.WriteLine("SAVING PROJECTS TO CACHE");
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20)
            };

            string serializedList = JsonSerializer.Serialize(projectTiles);
            await _cache.SetStringAsync(key, serializedList, options, cancellationToken);



            //Return in Response
            var response = (List<ProjectTileDto>)projectTiles;

            return Result<List<ProjectTileDto>>.Success(response, "Projects Retrieved Successfully");

        }
    }
}
