# 🚀 Hacker News Best Stories API

A production-ready ASP.NET Core Web API that retrieves the top **N best stories** from the official Hacker News API, ordered by score in descending order.

Built as a high-quality backend engineering solution with a strong focus on:

* ⚡ Performance
* 🧠 Concurrency control
* 🛡️ Resilience
* 📦 Containerization
* 🧪 Automated testing
* 📖 Professional API documentation
* 🏗️ Clean architecture principles

---

# 📋 Challenge Objective

Using ASP.NET Core, implement a RESTful API that:

* Retrieves best story IDs from Hacker News
* Fetches story details efficiently
* Returns the top **N stories** ordered by score
* Prevents overloading the Hacker News API
* Supports large request volumes
* Demonstrates production-quality software delivery practices

---

# 🏛️ Solution Highlights

## ✅ Core Features

* ASP.NET Core Web API (.NET 8)
* RESTful architecture
* Swagger / OpenAPI documentation
* Professional ProblemDetails error responses
* Dockerized deployment
* Health check endpoint
* Configurable options pattern
* Enterprise-grade logging

---

# ⚙️ Performance & Scalability Strategies

## 🧠 Intelligent Caching

To minimize upstream API pressure:

### In-memory caching:

* Best story IDs cached temporarily
* Individual story details cached
* Reduces redundant network calls
* Improves response times significantly

### Future production enhancement:

* Redis / distributed cache for horizontal scaling

---

## 🔄 Bounded Concurrency Control

Implemented using:

```csharp
SemaphoreSlim
```

### Benefits:

* Prevents unbounded parallel requests
* Avoids Hacker News API overload
* Improves stability under load
* Demonstrates distributed systems awareness

---

## 🛡️ Resilience with Polly

Configured:

* Retry policies
* Circuit breaker
* Timeout control

### Benefits:

* Handles transient upstream failures
* Improves API reliability
* Prevents cascading failures
* Production-grade service communication

---

# 🏗️ Architecture Overview

```txt
Controllers
 └── BestStoriesController

Services
 └── HnService

Models
 └── HnItem
 └── StoryDto

Options
 └── HackerNewsOptions
```

### Design Principles:

* SOLID-aligned
* Separation of concerns
* Dependency injection
* Configurability
* Testability
* Maintainability

---

# 🌐 API Endpoints

## 📌 Get Best Stories

```http
GET /api/v1/beststories?n=10
```

### Success Response:

```json
[
  {
    "title": "Example Story",
    "uri": "https://example.com",
    "postedBy": "author",
    "time": "2026-01-01T00:00:00+00:00",
    "score": 1234,
    "commentCount": 100
  }
]
```

---

# ❗ Error Responses

## 400 Bad Request

Invalid query parameter:

```json
{
  "title": "Invalid query parameter",
  "status": 400,
  "detail": "Parameter 'n' must be greater than zero."
}
```

---

## 502 Bad Gateway

Upstream Hacker News failure:

```json
{
  "title": "Upstream service error",
  "status": 502
}
```

---

## 500 Internal Server Error

Unexpected runtime failure:

```json
{
  "title": "Internal server error",
  "status": 500
}
```

---

# ❤️ Health Check

```http
GET /health
```

Returns:

```txt
Healthy
```

---

# 📚 Swagger Documentation

Available locally at:

```txt
/swagger
```

Includes:

* Request validation
* Response models
* Error contracts
* Status codes

---

# 🧪 Automated Testing

## Included:

### Unit Tests:

* Controller validation
* Error handling
* Service ordering
* Filtering
* Caching behavior
* Upstream failures

### Integration Tests:

* Full endpoint execution
* Swagger availability
* Health endpoint
* Real API contract validation

---

## Run tests:

```bash
dotnet test
```

---

# 🐳 Docker Deployment

## Build image:

```bash
docker build -t best-stories-api .
```

## Run container:

```bash
docker run --rm -p 8080:8080 best-stories-api
```

---

# 🔐 Container Security

### Security best practices implemented:

* Non-root runtime user
* Multi-stage build
* Smaller runtime image
* Reduced attack surface

---

# 💻 Local Development

## Requirements:

* .NET 8 SDK
* Docker (optional)

---

## Run locally:

```bash
dotnet restore
dotnet run --project BestStoriesApi/BestStoriesApi.csproj
```

---

# 📈 Trade-offs & Assumptions

## Assumptions:

* Maximum `n` capped at 500
* Candidate multiplier used to improve score accuracy without fetching all stories
* Memory cache acceptable for single-instance deployment
* Hacker News availability is external dependency

---

## Trade-offs:

### Chosen:

* Faster responses
* Reduced upstream pressure
* Controlled memory usage

### Sacrificed:

* Perfect real-time score precision for all global stories
* Multi-node cache consistency (future Redis enhancement)

---

# 🔮 Future Improvements

## Potential Enhancements:

* Redis distributed caching
* Rate limiting / throttling
* Authentication & authorization
* Prometheus metrics
* OpenTelemetry tracing
* CI/CD pipeline automation
* GitHub Actions
* Kubernetes deployment
* Advanced observability dashboards
* Response compression
* Distributed tracing

---

# 🏁 Final Notes

This project was intentionally engineered beyond basic functional requirements to demonstrate:

* Senior-level .NET backend development
* Distributed systems awareness
* Performance optimization
* API professionalism
* Production readiness
* Security best practices
* Enterprise software delivery standards

---

# 👨‍💻 Author

Developed as a technical coding challenge solution with enterprise-grade implementation standards.
