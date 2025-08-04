@echo off
echo Aplicando migración PostgreSQL a Railway...

REM Configurar variable de entorno para usar PostgreSQL
set ASPNETCORE_ENVIRONMENT=Production

REM Aplicar migración
dotnet ef database update --connection "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=YwGewhgtdosfCLUDjogpTGSIGbGMisbW;SSL Mode=Require;Trust Server Certificate=true"

echo Migración completada
pause
