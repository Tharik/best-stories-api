using System.Text.Json;
using BestStoriesApi.Models;
using BestStoriesApi.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BestStoriesApi.Services;

public sealed class HnService : IHnService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HnService> _logger;
    private readonly HackerNewsOptions _options;
    private readonly SemaphoreSlim _semaphore;

    public HnService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<HnService> logger,
        IOptions<HackerNewsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
    }

    public async Task<IReadOnlyCollection<StoryDto>> GetBestStoriesAsync(
        int n,
        CancellationToken cancellationToken = default)
    {
        var ids = await GetBestStoryIdsAsync(cancellationToken);

        var candidateCount = Math.Min(
            ids.Count,
            Math.Max(n * _options.CandidateMultiplier, _options.MinimumCandidateCount));

        var selectedIds = ids.Take(candidateCount).ToArray();

        var tasks = selectedIds.Select(id =>
            GetStoryWithConcurrencyAsync(id, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.Score)
            .Take(n)
            .ToArray();
    }
    private async Task<List<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync("beststoryids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromSeconds(_options.BestStoryIdsCacheSeconds);

            var client = _httpClientFactory.CreateClient("hn");
            var response = await client.GetAsync("beststories.json", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonSerializer.Deserialize<List<int>>(json)
                   ?? new List<int>();
        }) ?? new List<int>();
    }

    private async Task<StoryDto?> GetStoryWithConcurrencyAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return await GetStoryAsync(id, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<StoryDto?> GetStoryAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync($"story_{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(_options.StoryCacheMinutes);

            var client = _httpClientFactory.CreateClient("hn");
            var response = await client.GetAsync($"item/{id}.json", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonSerializer.Deserialize<HnItem>(json);

            if (item is null || item.Type != "story")
                return null;

            return new StoryDto
            {
                Title = item.Title ?? string.Empty,
                Uri = item.Url,
                PostedBy = item.By ?? string.Empty,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
                Score = item.Score,
                CommentCount = item.Descendants
            };
        });
    }
}