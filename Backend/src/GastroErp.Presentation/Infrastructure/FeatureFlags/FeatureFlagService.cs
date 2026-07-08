namespace GastroErp.Presentation.Infrastructure.FeatureFlags;

public interface IFeatureFlagService
{
    Task<bool> IsFeatureEnabledAsync(string featureName, Guid tenantId);
}

public class FeatureFlagService : IFeatureFlagService
{
    public Task<bool> IsFeatureEnabledAsync(string featureName, Guid tenantId)
    {
        // Passive structure
        return Task.FromResult(true);
    }
}
