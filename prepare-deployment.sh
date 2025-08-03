# Script para preparar el proyecto para despliegue en la nube

# 1. Agregar paquete PostgreSQL para Railway/Render
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# 2. Compilar el proyecto
dotnet build

# 3. Publicar para producción
dotnet publish -c Release -o ./publish

echo "Proyecto preparado para despliegue"
echo "Archivos de configuración creados:"
echo "- appsettings.Production.json"
echo "- Dockerfile.production"
echo "- railway.yml"
echo "- deployment-guide.md"
