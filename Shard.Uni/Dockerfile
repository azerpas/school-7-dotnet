#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Shard.Uni/Shard.Uni.csproj", "Shard.Uni/"]
COPY ["Shard.Shared.Core/Shard.Shared.Core.csproj", "Shard.Shared.Core/"]
RUN dotnet restore "Shard.Uni/Shard.Uni.csproj"
COPY . .
WORKDIR "/src/Shard.Uni"
RUN dotnet build "Shard.Uni.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shard.Uni.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shard.Uni.dll"]