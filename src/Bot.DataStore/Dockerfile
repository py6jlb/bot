#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Bot.DataStore/Bot.DataStore.csproj", "src/Bot.DataStore/"]
COPY ["src/Bot.DataAccessLayer/Bot.DataAccessLayer.csproj", "src/Bot.DataAccessLayer/"]
RUN dotnet restore "src/Bot.DataStore/Bot.DataStore.csproj"
COPY . .
WORKDIR "/src/src/Bot.DataStore"
RUN dotnet build "Bot.DataStore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.DataStore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.DataStore.dll"]