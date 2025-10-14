# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "UserService.API.csproj"
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
<<<<<<< HEAD
ENTRYPOINT ["dotnet", "UserService.API.dll"]
=======
ENTRYPOINT ["dotnet", "UserService.API.dll"]
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
