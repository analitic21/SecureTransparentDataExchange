# =========================
# .NET 9 production Dockerfile (API on port 5000)
# =========================

# --- Base runtime image (ASP.NET Core 9) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Bind Kestrel to all interfaces on port 5000 (matches your ALB target group)
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Expose the container port that ALB will probe/forward to
EXPOSE 5000

# --- Build stage (SDK 9) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file first to leverage layer caching
COPY ["SecureTransparentDataExchange.csproj", "./"]

# Restore dependencies
RUN dotnet restore "./SecureTransparentDataExchange.csproj"

# Copy the rest of the source
COPY . .

# Build
RUN dotnet build "./SecureTransparentDataExchange.csproj" -c $BUILD_CONFIGURATION -o /app/build

# --- Publish stage ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SecureTransparentDataExchange.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Final image ---
FROM base AS final
WORKDIR /app

# Copy published files from the publish stage
COPY --from=publish /app/publish ./

# Container-level health check (hits your /health endpoint)
HEALTHCHECK --interval=20s --timeout=3s --retries=3 CMD \
  wget -qO- http://127.0.0.1:5000/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "SecureTransparentDataExchange.dll"]
