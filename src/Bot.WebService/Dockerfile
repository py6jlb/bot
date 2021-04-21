#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Bot.WebService/Bot.WebService.csproj", "src/Bot.WebService/"]
COPY ["src/Bot.UI/Shared/Shared.csproj", "src/Bot.UI/Shared/"]
RUN dotnet restore "src/Bot.WebService/Bot.WebService.csproj"
COPY . .
WORKDIR "/src/src/Bot.WebService"
RUN dotnet build "Bot.WebService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.WebService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.WebService.dll"]
