using BestStoriesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoriesApi.Controllers;

[ApiController]
[Route("api/v1/beststories")]
public class BestStoriesController : ControllerBase
{
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
    public async Task<IActionResult> GetBestStories(
        [FromQuery] int n = 10,
        CancellationToken cancellationToken = default)
    {
        if (n <= 0)
            return BadRequest("Parameter 'n' must be greater than zero.");

        if (n > 500)
            return BadRequest("Parameter 'n' must be less than or equal to 500.");

        try
        {
            var stories = await _hnService.GetBestStoriesAsync(n, cancellationToken);
            return Ok(stories);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Hacker News upstream failure.");
            return StatusCode(502, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving best stories.");
            return StatusCode(500, "Internal server error.");
        }
    }
}