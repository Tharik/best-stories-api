FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BestStoriesApi/BestStoriesApi.csproj", "BestStoriesApi/"]
RUN dotnet restore "BestStoriesApi/BestStoriesApi.csproj"

COPY . .
RUN dotnet publish "BestStoriesApi/BestStoriesApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BestStoriesApi.dll"]