# Worker Service için .NET Runtime imajı kullanıyoruz
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG LAUNCHING_FROM_VS=false
WORKDIR /src
COPY ["src/OrderManagement.WorkerService/OrderManagement.WorkerService.csproj", "src/OrderManagement.WorkerService/"]
COPY ["src/OrderManagement.Core/OrderManagement.Core.csproj", "src/OrderManagement.Core/"]
COPY ["src/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/OrderManagement.Infrastructure/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "src/OrderManagement.WorkerService/OrderManagement.WorkerService.csproj"
COPY . .
WORKDIR "/src/src/OrderManagement.WorkerService"
RUN dotnet build "OrderManagement.WorkerService.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrderManagement.WorkerService.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderManagement.WorkerService.dll"]