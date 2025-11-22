@echo off
echo Iniciando Isodoc...
echo Acesse em: https://localhost:5001
"C:\Program Files\dotnet\dotnet.exe" run --project "c:\Isodoc_Asp.Net\Isodoc.Web\Isodoc.Web.csproj" --urls=https://localhost:5001
pause
