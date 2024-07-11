FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PRODUCER/PRODUCER.csproj", "PRODUCER/"]
RUN dotnet restore "PRODUCER/PRODUCER.csproj"
COPY . .
WORKDIR "/src/PRODUCER"
RUN dotnet build "PRODUCER.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PRODUCER.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PRODUCER.dll"]
