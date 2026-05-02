using BestStoriesApi.Models;
using BestStoriesApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BestStoriesApi.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var existingService = services.SingleOrDefault(
                x => x.ServiceType == typeof(IHnService));

            if (existingService is not null)
            {
                services.Remove(existingService);
            }

            var hnServiceMock = new Mock<IHnService>();

            hnServiceMock
                .Setup(x => x.GetBestStoriesAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StoryDto>
                {
                    new()
                    {
                        Title = "Integration Test Story",
                        Uri = "https://example.com",
                        PostedBy = "integration-user",
                        Time = DateTimeOffset.UtcNow,
                        Score = 100,
                        CommentCount = 10
                    }
                });

            services.AddSingleton(hnServiceMock.Object);
        });
    }
}