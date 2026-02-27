using TaskManager.Application.UserConnections.DTOs;

namespace TaskManager.Presentation.Services
{
    public class AssigneeListStateService
    {
        private List<UserConnectionDto> _assigneeCache = new List<UserConnectionDto>();

        public List<UserConnectionDto>? GetAssigneesFromCache()
        {
            if (_assigneeCache.Any())
            {
                return _assigneeCache;
            }
            return null;
        }
         
        public void SetAssigneesInCache(List<UserConnectionDto> assignees)
        {
            if(assignees.Any())
            {
                _assigneeCache = assignees;
            }
        }

        public void SetAssigneeInCache(UserConnectionDto assignee)
        {
            if (assignee is not null)
            {
                _assigneeCache.Add(assignee);
            }
        }

        public void RemoveFromCache(UserConnectionDto connection)
        {
             _assigneeCache.Remove(connection);
        }
    }
}
