namespace TaskManager.Application.Projects.DTOs.Responses
{
    public record CreateProjectResponse
    {
        public ProjectTileDto? CreatedProject { get; init; }
       // public string Message { get; set; } = string.Empty;
     
    }
}
