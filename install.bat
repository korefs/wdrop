@echo off
setlocal

echo 🔍 Verificando binário...

if not exist "out\wdrop.exe" (
    echo ❌ Arquivo 'wdrop.exe' não encontrado no diretório atual.
    echo Compile com:
    echo dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o out
    exit /b 1
)

echo 📁 Instalando wdrop globalmente...

REM Caminho comum de instalação para apps de terminal no Windows
set "TARGET=%USERPROFILE%\AppData\Local\Microsoft\WindowsApps"

copy /Y "out\wdrop.exe" "%TARGET%\wdrop.exe" >nul

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Falha ao copiar para %TARGET%.
    echo Execute este script como Administrador ou copie manualmente.
    exit /b 1
)

echo ✅ wdrop instalado com sucesso!
echo Use: wdrop <file>

endlocal