#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Bot.DataStore/Bot.DataStore.csproj", "Bot.DataStore/"]
COPY ["Bot.DataAccessLayer/Bot.DataAccessLayer.csproj", "Bot.DataAccessLayer/"]
RUN dotnet restore "Bot.DataStore/Bot.DataStore.csproj"
COPY . .
WORKDIR "/src/Bot.DataStore"
RUN dotnet build "Bot.DataStore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.DataStore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.DataStore.dll"]
