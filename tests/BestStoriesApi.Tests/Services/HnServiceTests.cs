using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using BestStoriesApi.Options;
using BestStoriesApi.Services;

namespace BestStoriesApi.Tests.Services;

public sealed class HnServiceTests
{
    private static HnService CreateService(
        Dictionary<string, string> responses,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(responses, statusCode);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        httpClientFactoryMock
            .Setup(x => x.CreateClient("hn"))
            .Returns(httpClient);

        var cache = new MemoryCache(new MemoryCacheOptions());

        var loggerMock = new Mock<ILogger<HnService>>();

        var options = Microsoft.Extensions.Options.Options.Create(new HackerNewsOptions
        {
            MaxStories = 500,
            MaxConcurrentRequests = 4,
            MinimumCandidateCount = 3,
            CandidateMultiplier = 1,
            BestStoryIdsCacheSeconds = 60,
            StoryCacheMinutes = 10
        });

        return new HnService(
            httpClientFactoryMock.Object,
            cache,
            loggerMock.Object,
            options);
    }

    private static string StoryJson(
        int id,
        string title,
        int score,
        string type = "story")
    {
        return "{ " +
               $"\"id\": {id}, " +
               $"\"type\": \"{type}\", " +
               $"\"title\": \"{title}\", " +
               $"\"by\": \"user{id}\", " +
               "\"time\": 1700000000, " +
               $"\"score\": {score}, " +
               $"\"descendants\": {id}, " +
               $"\"url\": \"https://example.com/{id}\" " +
               "}";
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnStoriesOrderedByScoreDescending()
    {
        var responses = new Dictionary<string, string>
        {
            ["beststories.json"] = "[1,2,3]",
            ["item/1.json"] = StoryJson(1, "Low", 10),
            ["item/2.json"] = StoryJson(2, "High", 100),
            ["item/3.json"] = StoryJson(3, "Medium", 50)
        };

        var service = CreateService(responses);

        var result = await service.GetBestStoriesAsync(3);

        var stories = result.ToArray();

        Assert.Equal(3, stories.Length);
        Assert.Equal("High", stories[0].Title);
        Assert.Equal("Medium", stories[1].Title);
        Assert.Equal("Low", stories[2].Title);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnOnlyRequestedNumberOfStories()
    {
        var responses = new Dictionary<string, string>
        {
            ["beststories.json"] = "[1,2,3]",
            ["item/1.json"] = StoryJson(1, "Story 1", 100),
            ["item/2.json"] = StoryJson(2, "Story 2", 90),
            ["item/3.json"] = StoryJson(3, "Story 3", 80)
        };

        var service = CreateService(responses);

        var result = await service.GetBestStoriesAsync(2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldIgnoreItemsThatAreNotStories()
    {
        var responses = new Dictionary<string, string>
        {
            ["beststories.json"] = "[1,2,3]",
            ["item/1.json"] = StoryJson(1, "Valid Story", 100, "story"),
            ["item/2.json"] = StoryJson(2, "Invalid Comment", 90, "comment"),
            ["item/3.json"] = StoryJson(3, "Invalid Poll", 80, "poll")
        };

        var service = CreateService(responses);

        var result = await service.GetBestStoriesAsync(3);

        var story = Assert.Single(result);
        Assert.Equal("Valid Story", story.Title);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldThrowHttpRequestException_WhenUpstreamFails()
    {
        var responses = new Dictionary<string, string>
        {
            ["beststories.json"] = ""
        };

        var service = CreateService(responses, HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetBestStoriesAsync(3));
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldUseCache_WhenCalledTwice()
    {
        var responses = new Dictionary<string, string>
        {
            ["beststories.json"] = "[1]",
            ["item/1.json"] = StoryJson(1, "Cached Story", 100)
        };

        var handler = new FakeHttpMessageHandler(responses);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        httpClientFactoryMock
            .Setup(x => x.CreateClient("hn"))
            .Returns(httpClient);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var loggerMock = new Mock<ILogger<HnService>>();

        var options = Microsoft.Extensions.Options.Options.Create(new HackerNewsOptions
        {
            MaxStories = 500,
            MaxConcurrentRequests = 4,
            MinimumCandidateCount = 1,
            CandidateMultiplier = 1,
            BestStoryIdsCacheSeconds = 60,
            StoryCacheMinutes = 10
        });

        var service = new HnService(
            httpClientFactoryMock.Object,
            cache,
            loggerMock.Object,
            options);

        await service.GetBestStoriesAsync(1);
        await service.GetBestStoriesAsync(1);

        Assert.Equal(1, handler.GetRequestCount("beststories.json"));
        Assert.Equal(1, handler.GetRequestCount("item/1.json"));
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses;
        private readonly HttpStatusCode _statusCode;
        private readonly Dictionary<string, int> _requestCounts = new();

        public FakeHttpMessageHandler(
            Dictionary<string, string> responses,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responses = responses;
            _statusCode = statusCode;
        }

        public int GetRequestCount(string path)
        {
            return _requestCounts.TryGetValue(path, out var count)
                ? count
                : 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;

            if (path.StartsWith("v0/", StringComparison.OrdinalIgnoreCase))
            {
                path = path["v0/".Length..];
            }

            _requestCounts[path] = _requestCounts.GetValueOrDefault(path) + 1;

            if (_statusCode != HttpStatusCode.OK)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(string.Empty)
                });
            }

            if (!_responses.TryGetValue(path, out var response))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Empty)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response)
            });
        }
    }
}