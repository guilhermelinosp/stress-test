FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GATEWAY/GATEWAY.csproj", "GATEWAY/"]
RUN dotnet restore "GATEWAY/GATEWAY.csproj"
COPY . .
WORKDIR "/src/GATEWAY"
RUN dotnet build "GATEWAY.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GATEWAY.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GATEWAY.dll"]
