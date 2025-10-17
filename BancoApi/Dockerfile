# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar los archivos del proyecto
COPY . .

# Restaurar dependencias
RUN dotnet restore

# Compilar y publicar el proyecto en modo Release
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar los archivos publicados desde la etapa anterior
COPY --from=build /app/publish .

# Exponer el puerto que usa tu API (puedes ajustarlo si usas otro)
EXPOSE 8080

# Comando para ejecutar la API
ENTRYPOINT ["dotnet", "BancoApi.dll"]
