@echo off
echo Running Gurobi optimization in WSL...

REM Check if WSL script exists
if not exist "%~dp0setup_gurobi_wsl.sh" (
    echo Error: setup_gurobi_wsl.sh not found!
    exit /b 1
)

REM Check if Python script exists
if not exist "%~dp0wsl_gurobi_atamalar.py" (
    echo Error: wsl_gurobi_atamalar.py not found!
    exit /b 1
)

REM Get current directory in WSL-compatible format
FOR /F "tokens=* USEBACKQ" %%g IN (`wsl wslpath '%~dp0'`) DO (SET "WSL_PATH=%%g")
echo WSL path: %WSL_PATH%

REM Create project directory in WSL
echo Creating project directory in WSL...
wsl mkdir -p ~/gurobi_project

REM Copy files to WSL home directory
echo Copying files to WSL...
wsl cp "%WSL_PATH%setup_gurobi_wsl.sh" ~/gurobi_project/
wsl cp "%WSL_PATH%wsl_gurobi_atamalar.py" ~/gurobi_project/

REM Make script executable
wsl chmod +x ~/gurobi_project/setup_gurobi_wsl.sh

REM Run setup script
echo Installing Gurobi in WSL (this may take a while)...
wsl bash ~/gurobi_project/setup_gurobi_wsl.sh

REM Run Python script using the optimizer script
echo Running optimization script...
wsl bash ~/gurobi_project/run_optimizer.sh

REM Copy results back to Windows
echo Copying results back to Windows...
wsl cp ~/gurobi_project/wsl_driver_schedule.json "%~dp0wsl_driver_schedule.json"

echo Done!
pause 