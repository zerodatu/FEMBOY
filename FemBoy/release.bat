@echo off
setlocal EnableExtensions

rem スクリプト自身のディレクトリで実行する
cd /d "%~dp0"

rem 安全に出力ディレクトリを掃除してから publish を実行する
if exist ".\dist\win" rmdir /s /q ".\dist\win"
if exist ".\dist\linux" rmdir /s /q ".\dist\linux"

mkdir ".\dist\win" 2>nul
if errorlevel 1 goto :fail

mkdir ".\dist\linux" 2>nul
if errorlevel 1 goto :fail

echo Publishing win-x64 to .\dist\win
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true --self-contained true -o .\dist\win
if errorlevel 1 goto :fail

echo Publishing linux-x64 to .\dist\linux
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true --self-contained true -o .\dist\linux
if errorlevel 1 goto :fail

echo Release completed successfully.
exit /b 0

:fail
echo Release failed with exit code %errorlevel%.
exit /b %errorlevel%
