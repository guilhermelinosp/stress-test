FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ["CONSUMER/CONSUMER.csproj", "CONSUMER/"]
RUN dotnet restore "CONSUMER/CONSUMER.csproj"
COPY . .
WORKDIR "/source/CONSUMER"
RUN dotnet build "CONSUMER.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CONSUMER.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CONSUMER.dll"]
