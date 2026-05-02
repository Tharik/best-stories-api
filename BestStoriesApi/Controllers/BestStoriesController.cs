using BestStoriesApi.Models;
using BestStoriesApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BestStoriesApi.Controllers;

[ApiController]
[Route("api/v1/beststories")]
[Produces("application/json")]
public class BestStoriesController : ControllerBase
{
    private const int MaxStories = 500;

    private readonly IHnService _hnService;
    private readonly ILogger<BestStoriesController> _logger;

    public BestStoriesController(
        IHnService hnService,
        ILogger<BestStoriesController> logger)
    {
        _hnService = hnService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves the best Hacker News stories",
        Description = "Returns the top N Hacker News stories ordered by score in descending order.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBestStories(
        [FromQuery] int n = 10,
        CancellationToken cancellationToken = default)
    {
        if (n <= 0)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Invalid query parameter",
                "Parameter 'n' must be greater than zero."));
        }

        if (n > MaxStories)
        {
            return BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Invalid query parameter",
                $"Parameter 'n' must be less than or equal to {MaxStories}."));
        }

        try
        {
            var stories = await _hnService.GetBestStoriesAsync(n, cancellationToken);
            return Ok(stories);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Hacker News upstream failure.");

            return StatusCode(
                StatusCodes.Status502BadGateway,
                CreateProblemDetails(
                    StatusCodes.Status502BadGateway,
                    "Upstream service error",
                    "The Hacker News API could not be reached or returned an invalid response."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving best stories.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred while processing the request."));
        }
    }

    private ProblemDetails CreateProblemDetails(
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };
    }
}