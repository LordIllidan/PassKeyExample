# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PasskeyAuth.Api.csproj", "./"]
RUN dotnet restore "PasskeyAuth.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "PasskeyAuth.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PasskeyAuth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install krb5 libraries for PostgreSQL/Npgsql
RUN apt-get update && \
    apt-get install -y --no-install-recommends krb5-locales libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 5200

ENTRYPOINT ["dotnet", "PasskeyAuth.Api.dll"]
