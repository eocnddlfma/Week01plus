@echo off
REM This script triggers Unity to generate orbital weapon prefabs
REM The prefabs will be created in Assets/03. Prefab/Satellites/

REM Find Unity Editor executable
FOR /F "delims=" %%i IN ('where unity.exe 2^>nul') DO set UNITY_EXE=%%i

IF NOT DEFINED UNITY_EXE (
    echo Unity Editor not found in PATH. Please run manually:
    echo 1. Open the project in Unity Editor
    echo 2. Go to Tools menu and click "Generate Special Orbital Weapon Prefabs"
    echo 3. The prefabs will be created in Assets/03. Prefab/Satellites/
    exit /b 1
)

REM Execute the menu item through Unity command line
"%UNITY_EXE%" -projectPath "%CD%" -executeMethod OrbitalWeaponPrefabGenerator.GenerateOrbitalWeaponPrefabs -quit -batchmode

echo Orbital weapon prefabs generated successfully!
