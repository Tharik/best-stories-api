namespace BestStoriesApi.Options;

public sealed class HackerNewsOptions
{
    public int MaxStories { get; init; } = 500;
    public int MaxConcurrentRequests { get; init; } = 16;
    public int MinimumCandidateCount { get; init; } = 100;
    public int CandidateMultiplier { get; init; } = 3;
    public int BestStoryIdsCacheSeconds { get; init; } = 60;
    public int StoryCacheMinutes { get; init; } = 10;
}