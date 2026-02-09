# ============================================================
# Dockerfile cho VanBanPlus API
# Build multi-stage: SDK → Runtime (nhẹ hơn)
# ============================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj và restore
COPY AIVanBan.API/AIVanBan.API.csproj AIVanBan.API/
RUN dotnet restore AIVanBan.API/AIVanBan.API.csproj

# Copy source và build
COPY AIVanBan.API/ AIVanBan.API/
RUN dotnet publish AIVanBan.API/AIVanBan.API.csproj -c Release -o /app/publish

# Stage 2: Runtime (nhẹ ~200MB thay vì 800MB)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Tạo thư mục data cho LiteDB
RUN mkdir -p /app/Data

COPY --from=build /app/publish .

# Port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AIVanBan.API.dll"]
