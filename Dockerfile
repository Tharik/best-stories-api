FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BestStoriesApi/BestStoriesApi.csproj", "src/BestStoriesApi/"]
RUN dotnet restore "src/BestStoriesApi/BestStoriesApi.csproj"

COPY . .
RUN dotnet publish "src/BestStoriesApi/BestStoriesApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BestStoriesApi.dll"]