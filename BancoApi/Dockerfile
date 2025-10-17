# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar todo el proyecto
COPY . .

# Restaurar dependencias
RUN dotnet restore

# Compilar y publicar
RUN dotnet publish -c Release -o /app

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Puerto en el que correrá la API
EXPOSE 8080

# Reemplaza NOMBRE_DE_TU_DLL.dll por el nombre real de tu DLL
ENTRYPOINT ["dotnet", "BancoApi.dll"]
