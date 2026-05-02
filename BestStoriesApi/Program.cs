using System.Net.Http.Headers;
using BestStoriesApi.Services;
using BestStoriesApi.Options;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection("HackerNews"));

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("hn", client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retry =>
        TimeSpan.FromMilliseconds(200 * retry)))
.AddTransientHttpErrorPolicy(policy =>
    policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddSingleton<IHnService, HnService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}