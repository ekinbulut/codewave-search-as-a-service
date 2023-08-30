FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Publisher/Publisher.csproj", "Publisher/"]
RUN dotnet restore "Publisher/Publisher.csproj"
COPY . .
WORKDIR "/src/Publisher"
RUN dotnet build "Publisher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Publisher.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Publisher.dll"]
