using BestStoriesApi.Controllers;
using BestStoriesApi.Models;
using BestStoriesApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BestStoriesApi.Tests.Controllers;

public sealed class BestStoriesControllerTests
{
    private readonly Mock<IHnService> _hnServiceMock = new();
    private readonly Mock<ILogger<BestStoriesController>> _loggerMock = new();

    private BestStoriesController CreateController()
    {
        var controller = new BestStoriesController(
            _hnServiceMock.Object,
            _loggerMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnOk_WhenNIsValid()
    {
        var stories = new List<StoryDto>
        {
            new()
            {
                Title = "Test story",
                Uri = "https://example.com",
                PostedBy = "james",
                Time = DateTimeOffset.UtcNow,
                Score = 100,
                CommentCount = 10
            }
        };

        _hnServiceMock
            .Setup(x => x.GetBestStoriesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stories);

        var controller = CreateController();

        var result = await controller.GetBestStories(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var value = Assert.IsAssignableFrom<IReadOnlyCollection<StoryDto>>(okResult.Value);
        Assert.Single(value);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnBadRequest_WhenNIsZero()
    {
        var controller = CreateController();

        var result = await controller.GetBestStories(0);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid query parameter", problem.Title);
        Assert.Equal("Parameter 'n' must be greater than zero.", problem.Detail);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnBadRequest_WhenNIsGreaterThanMax()
    {
        var controller = CreateController();

        var result = await controller.GetBestStories(999);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid query parameter", problem.Title);
        Assert.Equal("Parameter 'n' must be less than or equal to 500.", problem.Detail);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnBadGateway_WhenServiceThrowsInvalidOperationException()
    {
        _hnServiceMock
            .Setup(x => x.GetBestStoriesAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Upstream failed"));

        var controller = CreateController();

        var result = await controller.GetBestStories(10);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status502BadGateway, problem.Status);
        Assert.Equal("Upstream service error", problem.Title);
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        _hnServiceMock
            .Setup(x => x.GetBestStoriesAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var controller = CreateController();

        var result = await controller.GetBestStories(10);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("Internal server error", problem.Title);
    }
}