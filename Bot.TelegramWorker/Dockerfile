#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Bot.TelegramWorker/Bot.TelegramWorker.csproj", "Bot.TelegramWorker/"]
RUN dotnet restore "Bot.TelegramWorker/Bot.TelegramWorker.csproj"
COPY . .
WORKDIR "/src/Bot.TelegramWorker"
RUN dotnet build "Bot.TelegramWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.TelegramWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.TelegramWorker.dll"]
