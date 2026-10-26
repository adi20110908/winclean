@echo off
setlocal EnableExtensions
:: ##release: 2026.03.23
REM LINT:IGNORE W012, W030
:: TUDJA AZ ÉKEZETEKET!!
:: version slogen: folderlocker -- betaversion -- 1% //loggerupdate //recommenedapps

set "ver=v4.4.0"

:: ================================ BOOT ================================ ::
title winclean booting...
chcp 65001 >nul
cd /d "%~dp0"
mode con: cols=78 lines=40 >nul 2>&1

:: admin rights
fltmc >nul 2>&1
if errorlevel 1 goto :need_admin
goto :init
:need_admin
color C1
echo.
echo   winclean needs administrator rights.
echo   requesting elevation...
echo.
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs" >nul 2>&1
timeout /t 3 >nul
exit /b

:init

set "WC_DIR=%APPDATA%\winclean"
set "LOG_DIR=%WC_DIR%\logs"
set "MAIN_LOG=%LOG_DIR%\winclean.log"
set "LATEST_LOG=%LOG_DIR%\winclean-latest.log"
set "COLOR_FILE=%WC_DIR%\colormode.cfg"
set "PROFILE_FILE=%WC_DIR%\colorprofile.cfg"
set "USER_FILE=%WC_DIR%\user.cfg"
set "LOCK_FILE=%WC_DIR%\locker.pw"

if not exist "%WC_DIR%" mkdir "%WC_DIR%"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

set "color=B"
set "colorprofile=Light Aqua"
set "user=User"

if exist "%COLOR_FILE%" set /p color=<"%COLOR_FILE%"
if exist "%PROFILE_FILE%" set /p colorprofile=<"%PROFILE_FILE%"
if exist "%USER_FILE%" set /p user=<"%USER_FILE%"

if not defined color set "color=B"
if not defined colorprofile set "colorprofile=Light Aqua"
if not defined user set "user=User"

:: broken color values fall back to the default
set "color=%color:"=%"
echo(%color%| findstr /r /x "[0-9A-Fa-f][0-9A-Fa-f]*" >nul 2>&1
if errorlevel 1 set "color=B"

if not exist "%MAIN_LOG%" (
    echo WINCLEAN LOG>"%MAIN_LOG%"
    echo Created: %DATE% %TIME%>>"%MAIN_LOG%"
)
> "%LATEST_LOG%" echo(=== WINCLEAN - LATEST SESSION ===
>>"%LATEST_LOG%" echo(Started: %DATE% %TIME%
>>"%LATEST_LOG%" echo(Version: %ver%

cls
echo.
echo   starting winclean %ver%...
echo   [ok] config   color=%colorprofile%
echo   [ok] logs     %LOG_DIR%

call :log "boot started"
call :log "color=%colorprofile% (%color%)"
call :log_device

title winclean %ver%
color %color%

:: ================================ MENU ================================ ::
:menu
cls
color %color%
title winclean %ver%
echo  +==================================================================+
echo    WINCLEAN %ver%   -   MAIN MENU
call :echo_user
echo  +==================================================================+
echo.
echo    CLEANING
echo      1  Temp files                user + system + public temp
echo      2  Old Windows Update files  SoftwareDistribution + DISM
echo      3  Disk cleanup              cleanmgr, all profiles
echo      4  Repair system files       DISM RestoreHealth + SFC
echo      5  Winget app updates        upgrade every package
echo      6  Driver check / update     pnputil + Windows Update
echo.
echo    TOOLS
echo      7  Folder locker             lock / unlock with password
echo      8  Disk health               chkdsk
echo      9  RAM check                 Windows memory diagnostic
echo      A  Deep clean + repair       1 + 2 + 3 + 4
echo.
echo    OTHER
echo      B  Change color
echo      C  Change username
echo      D  Recommended apps
echo      E  Show log
echo      R  Run everything, then exit
echo      S  Restart winclean
echo      0  Exit
echo.
choice /c 123456789ABCDERS0 /n /m "  select > "
set "SEL=%errorlevel%"

if "%SEL%"=="16" goto :do_restart
if "%SEL%"=="17" goto :do_exit
if "%SEL%"=="15" call :opt_runall
if "%SEL%"=="15" goto :do_exit
if "%SEL%"=="14" call :opt_log
if "%SEL%"=="13" call :opt_apps
if "%SEL%"=="12" call :opt_user
if "%SEL%"=="11" call :opt_color
if "%SEL%"=="10" call :opt_deep
if "%SEL%"=="9"  call :opt_ram
if "%SEL%"=="8"  call :opt_disk
if "%SEL%"=="7"  call :opt_locker
if "%SEL%"=="6"  call :opt_driver
if "%SEL%"=="5"  call :opt_winget
if "%SEL%"=="4"  call :opt_repair
if "%SEL%"=="3"  call :opt_cleanmgr
if "%SEL%"=="2"  call :opt_wuclean
if "%SEL%"=="1"  call :opt_temp
goto :menu

:: ============================== RESTART =============================== ::
:do_restart
call :log "winclean restarting..."
start "" "%~f0"
exit /b

:: ================================ EXIT ================================ ::
:do_exit
call :log "winclean closed"
title winclean exiting
cls
echo.
echo   thanks for using winclean!
echo.
timeout /t 1 >nul
exit /b

:: =============================== HELPERS ============================== ::

:head
cls
color %color%
echo  +==================================================================+
echo    WINCLEAN %ver%   -   %~1
call :echo_user
echo  +==================================================================+
echo.
goto :EOF

:echo_user
set /p "=    user: %user%" <nul
echo.
goto :EOF

:finish
echo.
set "rate="
set /p "rate=  rate winclean 1-10 (enter = skip): "
set "rate=%rate:"=%"
if not defined rate goto :rate_done
set "rated=0"
for %%R in (1 2 3 4 5 6 7 8 9 10) do if "%rate%"=="%%R" set "rated=1"
if "%rated%"=="0" (
    echo   please use a number from 1 to 10 - rating skipped.
    goto :rate_done
)
call :log "rated %rate%"
echo   thanks for the %rate%/10!
:rate_done
echo.
echo   press any key to return to the menu...
pause >nul
goto :EOF

:log
set "LINE=[%DATE% %TIME:~0,8%] %~1"
set /p "=%LINE%" <nul >>"%MAIN_LOG%"
echo.>>"%MAIN_LOG%"
set /p "=%LINE%" <nul >>"%LATEST_LOG%"
echo.>>"%LATEST_LOG%"
goto :EOF

:log_device
call :log "devicename: %COMPUTERNAME%"
goto :EOF

:save_config
> "%COLOR_FILE%" set /p "=%color%" <nul
> "%PROFILE_FILE%" set /p "=%colorprofile%" <nul
goto :EOF

:clean_dir
if "%~1"=="" goto :EOF
if not exist "%~1\" goto :EOF
del /s /f /q "%~1\*" >nul 2>&1
for /d %%D in ("%~1\*") do if exist "%%D\" rd /s /q "%%D" 2>nul
goto :EOF

:: ================================ STEPS =============================== ::

:step_temp
echo   - user temp: %TEMP%
call :clean_dir "%TEMP%"
echo   - system temp: %SystemRoot%\Temp
call :clean_dir "%SystemRoot%\Temp"
echo   - public temp: %PUBLIC%\temp
call :clean_dir "%PUBLIC%\temp"
echo   done.
goto :EOF

:step_wuclean
echo   - stopping Windows Update and BITS...
net stop wuauserv /y >nul 2>&1
net stop bits /y >nul 2>&1
echo   - deleting downloaded updates: %SystemRoot%\SoftwareDistribution\Download
call :clean_dir "%SystemRoot%\SoftwareDistribution\Download"
echo   - cleaning the component store (this can take a few minutes)...
Dism.exe /online /Cleanup-Image /StartComponentCleanup
echo   - restarting services...
net start bits >nul 2>&1
net start wuauserv >nul 2>&1
echo   done.
goto :EOF

:step_cleanmgr
if not exist "%SystemRoot%\System32\cleanmgr.exe" (
    echo   - cleanmgr.exe not found on this system, skipped.
    goto :EOF
)
echo   - preparing disk cleanup profiles...
for /f "delims=" %%K in ('reg query "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches" 2^>nul ^| findstr /b /c:"HKEY_"') do reg add "%%K" /v StateFlags0001 /t REG_DWORD /d 2 /f >nul 2>&1
echo   - running Disk Cleanup, a window will appear...
cleanmgr /sagerun:1
echo   done.
goto :EOF

:step_repair
echo   - step 1/2: DISM RestoreHealth (can take 5-10 minutes)
Dism.exe /online /Cleanup-Image /RestoreHealth
echo   - step 2/2: System File Checker
sfc /scannow
echo   done.
goto :EOF

:step_winget
where winget >nul 2>&1
if errorlevel 1 (
    echo   - winget is not available on this system, skipped.
    goto :EOF
)
echo   - checking for app updates...
winget upgrade --all --accept-package-agreements --accept-source-agreements
echo   done.
goto :EOF

:: =============================== OPTIONS ============================== ::

:opt_temp
call :head "TEMP FILES"
call :log "temp cleanup started"
call :step_temp
call :log "temp cleanup finished"
goto :finish

:opt_wuclean
call :head "OLD WINDOWS UPDATE FILES"
call :log "old windows update cleanup started"
call :step_wuclean
call :log "old windows update cleanup finished"
goto :finish

:opt_cleanmgr
call :head "DISK CLEANUP"
call :log "disk cleanup started"
call :step_cleanmgr
call :log "disk cleanup finished"
goto :finish

:opt_repair
call :head "REPAIR SYSTEM FILES"
call :log "repair started"
call :step_repair
call :log "repair finished"
goto :finish

:opt_winget
call :head "WINGET UPDATES"
call :log "winget update started"
call :step_winget
call :log "winget update finished"
goto :finish

:opt_driver
call :head "DRIVER CHECK / UPDATE"
call :log "driver check opened"
set "drvcount=0"
for /f %%N in ('pnputil /enum-drivers 2^>nul ^| find /c "oem"') do set "drvcount=%%N"
echo   %drvcount% driver packages are in the driver store.
echo.
echo   Drivers are updated through Windows Update or the pc maker's tool.
echo.
echo    1  Open Windows Update     (recommended)
echo    2  List installed drivers  (pnputil)
echo    0  Back
echo.
choice /c 120 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="3" goto :EOF
if "%SUBSEL%"=="2" goto :driver_list
echo.
echo   opening Windows Update...
start "" ms-settings:windowsupdate
goto :finish
:driver_list
echo.
pnputil /enum-drivers | more
goto :finish

:opt_locker
call :head "FOLDER LOCKER"
call :log "folder locker opened"
echo   Locking hides the folder and blocks access until you unlock it.
echo   The password is stored in: %LOCK_FILE%
echo.
echo    1  Lock a folder
echo    2  Unlock a folder
echo    0  Back
echo.
choice /c 120 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="3" goto :EOF
if "%SUBSEL%"=="2" goto :locker_unlock
goto :locker_lock

:locker_lock
echo.
set "target="
set /p "target=  folder to lock (full path): "
set "target=%target:"=%"
if not defined target goto :EOF
if not exist "%target%\" (
    echo   [!] not a folder: %target%
    goto :EOF
)
for %%R in (C D E F G H I J K L M N O P Q R S T U V W X Y Z) do if /i "%target%"=="%%R:\" goto :locker_guard
if /i "%target%"=="%SystemRoot%" goto :locker_guard
if /i "%target%"=="%SystemRoot%\System32" goto :locker_guard
if /i "%target%"=="%SystemRoot%\SysWOW64" goto :locker_guard
if /i "%target%"=="%APPDATA%" goto :locker_guard
if /i "%target%"=="%WC_DIR%" goto :locker_guard
if /i "%target%"=="%USERPROFILE%" goto :locker_guard

set "pw="
set /p "pw=  password: "
set "pw=%pw:"=%"
if not defined pw (
    echo   [!] a password is required.
    goto :EOF
)
set "pw2="
set /p "pw2=  repeat password: "
set "pw2=%pw2:"=%"
if not "%pw%"=="%pw2%" (
    echo   [!] the passwords do not match.
    goto :EOF
)
> "%LOCK_FILE%" set /p "=%pw%" <nul

echo   locking %target% ...
attrib +h "%target%" >nul 2>&1
icacls "%target%" /deny "*S-1-1-0:(OI)(CI)(F)" >nul 2>&1
if errorlevel 1 (
    attrib -h "%target%" >nul 2>&1
    echo   [!] icacls failed, the folder was not locked.
    goto :EOF
)
call :log "folder locked"
echo   [ok] locked. use menu 7 to unlock it with this password.
goto :finish

:locker_unlock
echo.
set "target="
set /p "target=  folder to unlock (full path): "
set "target=%target:"=%"
if not defined target goto :EOF
if not exist "%target%\" (
    echo   [!] not a folder: %target%
    goto :EOF
)
set "pw="
set /p "pw=  password: "
set "pw=%pw:"=%"
set "stored="
if exist "%LOCK_FILE%" set /p stored=<"%LOCK_FILE%"
if not defined stored (
    echo   [!] no password saved yet, lock a folder first.
    goto :EOF
)
if not "%pw%"=="%stored%" (
    echo   [!] wrong password.
    call :log "folder unlock failed"
    goto :EOF
)
icacls "%target%" /remove:d *S-1-1-0 >nul 2>&1
attrib -h "%target%" >nul 2>&1
call :log "folder unlocked"
echo   [ok] unlocked.
goto :finish

:locker_guard
echo   [!] this folder is protected, pick another one.
goto :EOF

:opt_disk
call :head "DISK HEALTH"
call :log "disk check opened"
set "target="
set /p "target=  drive letter (example C): "
set "target=%target:"=%"
if not defined target goto :EOF
set "drive=%target:~0,1%:"
if not exist "%drive%\" (
    echo   [!] drive not found: %drive%
    goto :EOF
)
echo.
echo    1  Check only      read-only, only reports problems
echo    2  Check and fix   chkdsk /F, may ask for a restart
echo    0  Back
echo.
choice /c 120 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="3" goto :EOF
if "%SUBSEL%"=="1" goto :disk_check
echo.
chkdsk %drive% /F
goto :finish
:disk_check
echo.
chkdsk %drive%
goto :finish

:opt_ram
call :head "RAM CHECK"
call :log "ram check opened"
if not exist "%SystemRoot%\System32\mdsched.exe" (
    echo   [!] mdsched.exe not found on this system.
    goto :EOF
)
start "" "%SystemRoot%\System32\mdsched.exe"
echo   [ok] choose "Restart now" or "Schedule a restart" in the window.
goto :finish

:opt_deep
call :head "DEEP CLEAN + REPAIR"
call :log "deep clean started"
echo   1/4 temp files
call :step_temp
echo.
echo   2/4 old windows update files
call :step_wuclean
echo.
echo   3/4 disk cleanup
call :step_cleanmgr
echo.
echo   4/4 repair system files
call :step_repair
call :log "deep clean finished"
goto :finish

:opt_runall
call :head "RUN EVERYTHING"
call :log "run all started"
echo   1/5 temp files
call :step_temp
echo.
echo   2/5 old windows update files
call :step_wuclean
echo.
echo   3/5 disk cleanup
call :step_cleanmgr
echo.
echo   4/5 repair system files
call :step_repair
echo.
echo   5/5 winget updates
call :step_winget
call :log "run all finished"
goto :EOF

:opt_color
call :head "COLOR"
echo   current: %colorprofile%  (%color%)
echo.
echo    1 Blue           2 Green          3 Aqua            4 Red
echo    5 Purple         6 Yellow         7 White           8 Gray
echo    9 Light Blue    A Light Green    B Light Aqua      C Light Red
echo    D Light Purple  E Light Yellow   F Bright White
echo.
echo    0  back
echo.
choice /c 123456789ABCDEF0 /n /m "  color > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="16" goto :EOF
set "COLORS=01:Blue 02:Green 03:Aqua 04:Red 05:Purple 06:Yellow 07:White 08:Gray 09:Light_Blue 0A:Light_Green 0B:Light_Aqua 0C:Light_Red 0D:Light_Purple 0E:Light_Yellow 0F:Bright_White"
for /f "tokens=%SUBSEL% delims= " %%C in ("%COLORS%") do set "PAIR=%%C"
for /f "tokens=1,2 delims=:" %%A in ("%PAIR%") do (set "color=%%A" & set "colorprofile=%%B")
set "colorprofile=%colorprofile:_= %"
color %color%
call :save_config
call :log "color changed: %colorprofile%"
echo.
echo   color set to %colorprofile%.
goto :finish

:opt_user
call :head "USERNAME"
call :log "username change opened"
set "newuser="
set /p "newuser=  new username: "
set "newuser=%newuser:"=%"
if not defined newuser goto :EOF
set "user=%newuser%"
> "%USER_FILE%" set /p "=%user%" <nul
call :log "username changed"
echo.
call :echo_user
goto :finish

:opt_apps
call :head "RECOMMENDED APPS"
call :log "downloader opened"
echo   Not sponsored, only recommended.
echo.
echo    1  Java       Oracle Java Runtime Environment
echo    2  LocalSend  local file sharing
echo    0  Back
echo.
choice /c 120 /n /m "  app > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="3" goto :EOF
if "%SUBSEL%"=="2" goto :app_localsend
echo.
echo   installing Java...
winget install --id=Oracle.JavaRuntimeEnvironment -e --accept-package-agreements --accept-source-agreements
goto :finish
:app_localsend
echo.
echo   installing LocalSend...
winget install localsend --accept-package-agreements --accept-source-agreements
goto :finish

:opt_log
call :head "LOG"
call :log "log opened"
if not exist "%MAIN_LOG%" (
    echo   no log file yet.
    goto :EOF
)
more < "%MAIN_LOG%"
goto :finish
