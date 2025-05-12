# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/OrderManagement.API/OrderManagement.API.csproj", "src/OrderManagement.API/"]
COPY ["src/OrderManagement.Core/OrderManagement.Core.csproj", "src/OrderManagement.Core/"]
COPY ["src/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/OrderManagement.Infrastructure/"]
COPY ["src/ProductCatalog.Core/ProductCatalog.Core.csproj", "src/ProductCatalog.Core/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj", "src/ProductCatalog.Infrastructure/"]
RUN dotnet restore "./src/OrderManagement.API/OrderManagement.API.csproj"
COPY . .
WORKDIR "/src/src/OrderManagement.API"
RUN dotnet build "./OrderManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]