#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Bot.UI/Server/Server.csproj", "src/Bot.UI/Server/"]
COPY ["src/Bot.UI/Shared/Shared.csproj", "src/Bot.UI/Shared/"]
COPY ["src/Bot.UI/Client/Client.csproj", "src/Bot.UI/Client/"]
RUN dotnet restore "src/Bot.UI/Server/Server.csproj"
COPY . .
WORKDIR "/src/src/Bot.UI/Server"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]