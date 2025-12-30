@echo off
setlocal

REM check hex2bin 
where hex2bin >nul 2>nul
if errorlevel 1 (
    echo can't find hex2bin.
    exit /b 1
)


set "found=0"
for %%f in (*.hex) do (
    set "found=1"
    set "hex_file=%%f"
   

    echo find %%f

   
    hex2bin "%%f" 
    
    if errorlevel 1 (
        echo fail to convert %%f 
    ) else (
        echo finish convert to %%f
    )
)

if %found%==0 (
    echo not find any hex
)
pause  
endlocal