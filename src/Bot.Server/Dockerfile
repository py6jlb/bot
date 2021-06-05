#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Bot.Server/Bot.Server.csproj", "src/Bot.Server/"]
COPY ["src/Bot.Client/Bot.Client.csproj", "src/Bot.Client/"]
COPY ["src/Bot.Shared/Bot.Shared.csproj", "src/Bot.Shared/"]
RUN dotnet restore "src/Bot.Server/Bot.Server.csproj"
COPY . .
WORKDIR "/src/src/Bot.Server"
RUN dotnet build "Bot.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.Server.dll"]