# Use .NET 10 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for caching restore)
COPY ["Cessa.API/Cessa.API.csproj", "Cessa.API/"]
COPY ["Cessa.Application/Cessa.Application.csproj", "Cessa.Application/"]
COPY ["Cessa.Domain/Cessa.Domain.csproj", "Cessa.Domain/"]
COPY ["Cessa.Infrastructure/Cessa.Infrastructure.csproj", "Cessa.Infrastructure/"]
COPY ["Cessa.Persistence/Cessa.Persistence.csproj", "Cessa.Persistence/"]
RUN dotnet restore "Cessa.API/Cessa.API.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/Cessa.API"
RUN dotnet build "Cessa.API.csproj" -c Release -o /app/build
RUN dotnet publish "Cessa.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET 10 runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create uploads folder
RUN mkdir -p /app/uploads

EXPOSE 8080
ENTRYPOINT ["dotnet", "Cessa.API.dll"]