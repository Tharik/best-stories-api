using BestStoriesApi.Models;

namespace BestStoriesApi.Services;

public interface IHnService
{
    Task<IReadOnlyCollection<StoryDto>> GetBestStoriesAsync(
        int n,
        CancellationToken cancellationToken = default);
}