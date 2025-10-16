@echo off
REM Cross-platform test script for Windows
REM Usage: scripts\test-windows.bat

setlocal enabledelayedexpansion

echo ========================================
echo LLM Capability Checker - Windows Test
echo ========================================
echo.

REM Check prerequisites
echo 1. Checking Prerequisites...

REM Check .NET SDK
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found
    echo Please install .NET 8.0 SDK from:
    echo   https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [32m√ .NET SDK found: %DOTNET_VERSION%[0m

REM Check WMI (built into Windows)
wmic os get caption >nul 2>&1
if %errorlevel% equ 0 (
    echo [32m√ WMI available[0m
) else (
    echo [33m! WMI check failed (may require admin rights)[0m
)

REM Check optional NVIDIA tools
where nvidia-smi >nul 2>&1
if %errorlevel% equ 0 (
    echo [32m√ nvidia-smi found (CUDA detection available)[0m
) else (
    echo [33m! nvidia-smi not found (CUDA detection disabled)[0m
)

REM Check DirectML support (Windows 10 build 18362+)
for /f "tokens=3" %%i in ('reg query "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion" /v CurrentBuild ^| findstr CurrentBuild') do set BUILD=%%i
if %BUILD% geq 18362 (
    echo [32m√ DirectML support available (Build %BUILD%)[0m
) else (
    echo [33m! DirectML may not be available (Build %BUILD% ^< 18362)[0m
)

echo.

REM Display platform information
echo 2. Platform Information:
for /f "tokens=*" %%i in ('wmic os get caption /value ^| findstr Caption') do (
    set "line=%%i"
    set "line=!line:Caption=!"
    echo   OS: !line!
)
for /f "tokens=*" %%i in ('wmic os get version /value ^| findstr Version') do (
    set "line=%%i"
    set "line=!line:Version=!"
    echo   Version: !line!
)
echo   Architecture: %PROCESSOR_ARCHITECTURE%
echo.

REM Build the project
echo 3. Building Project...
dotnet build src\LLMCapabilityChecker\LLMCapabilityChecker.csproj --configuration Debug

if %errorlevel% equ 0 (
    echo [32m√ Build successful[0m
) else (
    echo [31m× Build failed[0m
    exit /b 1
)
echo.

REM Run unit tests
echo 4. Running Unit Tests...
dotnet test tests\LLMCapabilityChecker.Tests\LLMCapabilityChecker.Tests.csproj --verbosity normal

if %errorlevel% equ 0 (
    echo [32m√ Unit tests passed[0m
) else (
    echo [31m× Unit tests failed[0m
    exit /b 1
)
echo.

REM Run service tester
echo 5. Running Service Tests...
echo --- Hardware Detection Test ---
dotnet run --project src\LLMCapabilityChecker -- --test

if %errorlevel% equ 0 (
    echo.
    echo [32m√ Service tests passed[0m
) else (
    echo.
    echo [31m× Service tests failed[0m
    exit /b 1
)
echo.

REM Summary
echo ========================================
echo [32mAll tests completed successfully![0m
echo ========================================
echo.
echo Next steps:
echo   - Run the full application: dotnet run --project src\LLMCapabilityChecker
echo   - Build release version: dotnet build --configuration Release
echo   - Review test results above for any warnings
echo.

REM Display detected hardware summary
echo Quick Hardware Summary:
for /f "tokens=2 delims==" %%i in ('wmic cpu get name /value ^| findstr Name') do (
    echo   CPU: %%i
)
for /f "tokens=2 delims==" %%i in ('wmic computersystem get totalphysicalmemory /value ^| findstr TotalPhysicalMemory') do (
    set /a MEM_GB=%%i / 1024 / 1024 / 1024
    echo   RAM: !MEM_GB! GB
)
for /f "tokens=2 delims==" %%i in ('wmic path win32_videocontroller get name /value ^| findstr Name') do (
    echo   GPU: %%i
    goto :gpu_done
)
:gpu_done
echo.

endlocal
