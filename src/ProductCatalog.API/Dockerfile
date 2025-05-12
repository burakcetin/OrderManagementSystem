# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG LAUNCHING_FROM_VS=false
WORKDIR /src
COPY ["src/ProductCatalog.API/ProductCatalog.API.csproj", "src/ProductCatalog.API/"]
COPY ["src/ProductCatalog.Core/ProductCatalog.Core.csproj", "src/ProductCatalog.Core/"]
COPY ["src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj", "src/ProductCatalog.Infrastructure/"]
COPY ["src/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/OrderManagement.Infrastructure/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/OrderManagement.Core/OrderManagement.Core.csproj", "src/OrderManagement.Core/"]

# Use the BUILD_CONFIGURATION parameter
RUN dotnet restore "src/ProductCatalog.API/ProductCatalog.API.csproj"
COPY . .
WORKDIR "/src/src/ProductCatalog.API"
RUN dotnet build "ProductCatalog.API.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ProductCatalog.API.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalog.API.dll"]