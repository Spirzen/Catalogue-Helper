# Базовый образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Создаем папку Data и даем права на запись
RUN mkdir -p /app/Data && chmod 777 /app/Data

EXPOSE 8080
EXPOSE 8081

# Сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CatalogueWebApp.csproj", "."]
RUN dotnet restore "CatalogueWebApp.csproj"
COPY . .
RUN dotnet build "CatalogueWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogueWebApp.csproj" -c Release -o /app/publish

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Копируем тестовые данные в папку Data
COPY test-data.json ./Data/