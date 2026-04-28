@echo off
REM ===============================
REM Auto Deploy Script for InfrastructureReportingSystem
REM ===============================

set BASE=%~dp0
set PROJECT=%BASE%InfrastructureReportingSystem\InfrastructureReportingSystem.csproj
set OUT_DIR=%BASE%Publish
set ZIP_PATH=%BASE%InfrastructureReportingSystem_Publish.zip

echo.
echo ================================================
echo  InfrastructureReportingSystem - Auto Deployment Starting.....
echo ================================================

echo.
echo ===============================
echo [0/3] - Cleaning old files.........
echo ===============================

REM Delete old Publish folder
if exist "%OUT_DIR%" (
		echo Deleting old Publish folder...
		rmdir /s /q "%OUT_DIR%"
)

REM Delete old zip file
if exist "%ZIP_PATH%" (
		echo Deleting old zip file...
		del /f /q "%ZIP_PATH%"
)

echo.
echo ===============================
echo [1/3] - Publishing project....
echo ===============================
dotnet publish "%PROJECT%" -c Release -r win-x64 -o "%OUT_DIR%"



echo.
echo ===============================
echo [3/3] - Creating zip file InfrastructureReportingSystem_Publish.zip ...
echo ===============================
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUT_DIR%\*' -DestinationPath '%ZIP_PATH%' -Force"

echo.
echo ===============================
echo [SUCCESS] Deployment completed successfully!
echo Deployment zip: %ZIP_PATH%
echo ===============================
pause
