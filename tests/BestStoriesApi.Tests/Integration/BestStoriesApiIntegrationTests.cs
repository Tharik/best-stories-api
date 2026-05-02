using System.Net;
using System.Net.Http.Json;
using BestStoriesApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BestStoriesApi.Tests.Integration;

public sealed class BestStoriesApiIntegrationTests 
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BestStoriesApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnOk_WhenRequestIsValid()
    {
        var response = await _client.GetAsync("/api/v1/beststories?n=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stories = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<StoryDto>>();

        Assert.NotNull(stories);
        Assert.Single(stories);
        Assert.Equal("Integration Test Story", stories.First().Title);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnBadRequest_WhenNIsZero()
    {
        var response = await _client.GetAsync("/api/v1/beststories?n=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Invalid query parameter", problem.Title);
        Assert.Equal("Parameter 'n' must be greater than zero.", problem.Detail);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnBadRequest_WhenNIsGreaterThanMax()
    {
        var response = await _client.GetAsync("/api/v1/beststories?n=999");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Invalid query parameter", problem.Title);
        Assert.Equal("Parameter 'n' must be less than or equal to 500.", problem.Detail);
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task Swagger_ShouldBeAvailableInDevelopment()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}