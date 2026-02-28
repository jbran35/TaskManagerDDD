using Microsoft.AspNetCore.Components.Server.Circuits;

namespace TaskManager.Presentation.Services
{
    public class CacheCleanupCircuitHandler(ProjectStateService cacheService) : CircuitHandler
    {
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            cacheService.ClearUserCache();
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
