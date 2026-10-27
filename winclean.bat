@echo off
setlocal EnableExtensions
:: ##release: 2026.10.27
REM LINT:IGNORE W012, W030
:: TUDJA AZ ÉKEZETEKET!!
:: version slogen: folderlocker -- betaversion -- 1% //loggerupdate //recommenedapps

set "ver=v4.5.0"

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

call :getfree

cls
echo.
echo   starting winclean %ver%...
echo   [ok] config   color=%colorprofile%
echo   [ok] disk     %WC_FREE% free on %SystemDrive%
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
echo      2  Browser cache             Chrome / Edge / Firefox cache only
echo      3  Old Windows Update files  update cache + component store
echo      4  Disk cleanup              cleanmgr, protected categories kept
echo      5  Recycle Bin               empty it, asks you first
echo      6  Repair system files       DISM RestoreHealth + SFC
echo      7  Winget app updates        upgrade every package
echo      8  Analyze disk              measure everything, delete nothing
echo      9  Deep clean + repair       1 + 3 + 4 + 6
echo.
echo    TOOLS
echo      A  Folder locker             lock / unlock with password
echo      B  Disk health               chkdsk
echo      C  RAM check                 Windows memory diagnostic
echo      D  Network fix               flush DNS / reset Winsock
echo      E  Restore point             snapshot before big changes
echo      F  System info               build, hardware, free space
echo.
echo    OTHER
echo      G  Driver check / update     list, back up, Windows Update
echo      H  Change color
echo      I  Change username
echo      J  Recommended apps
echo      K  Show log
echo      R  Run everything, then exit
echo      S  Restart winclean
echo      0  Exit
echo.
choice /c 123456789ABCDEFGHIJKRS0 /n /m "  select > "
set "SEL=%errorlevel%"

if "%SEL%"=="23" goto :do_exit
if "%SEL%"=="22" goto :do_restart
if "%SEL%"=="21" goto :opt_runall
if "%SEL%"=="20" goto :opt_log
if "%SEL%"=="19" goto :opt_apps
if "%SEL%"=="18" goto :opt_user
if "%SEL%"=="17" goto :opt_color
if "%SEL%"=="16" goto :opt_driver
if "%SEL%"=="15" goto :opt_sysinfo
if "%SEL%"=="14" goto :opt_restore
if "%SEL%"=="13" goto :opt_net
if "%SEL%"=="12" goto :opt_ram
if "%SEL%"=="11" goto :opt_disk
if "%SEL%"=="10" goto :opt_locker
if "%SEL%"=="9"  goto :opt_deep
if "%SEL%"=="8"  goto :opt_analyze
if "%SEL%"=="7"  goto :opt_winget
if "%SEL%"=="6"  goto :opt_repair
if "%SEL%"=="5"  goto :opt_recycle
if "%SEL%"=="4"  goto :opt_cleanmgr
if "%SEL%"=="3"  goto :opt_wuclean
if "%SEL%"=="2"  goto :opt_browser
if "%SEL%"=="1"  goto :opt_temp
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
set "WC_FREE_BEFORE=%WC_FREE_MB%"
cls
color %color%
echo  +==================================================================+
echo    WINCLEAN %ver%   -   %~1
call :echo_user
echo  +==================================================================+
echo.
goto :EOF

:echo_user
set /p "=    user: %user%   free: %WC_FREE%" <nul
echo.
goto :EOF

:finish
call :getfree
call :delta_free
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
goto :menu

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

:: free space of the system drive, in MB + readable form
:getfree
set "WC_FREE_MB="
for /f "delims=" %%B in ('powershell -NoProfile -Command "(Get-PSDrive %SystemDrive:~0,1%).Free/1MB -as [int]" 2^>nul') do set "WC_FREE_MB=%%B"
if not defined WC_FREE_MB set "WC_FREE_MB=0"
call :fmt_mb %WC_FREE_MB%
set "WC_FREE=%WC_TXT%"
goto :EOF

:delta_free
set "WC_DELTA="
if not defined WC_FREE_BEFORE goto :EOF
if not defined WC_FREE_MB goto :EOF
set /a "WC_DELTA=WC_FREE_MB-WC_FREE_BEFORE"
if not defined WC_DELTA goto :EOF
if %WC_DELTA% LEQ 0 goto :EOF
call :fmt_mb %WC_DELTA%
echo   freed: %WC_TXT%   (free space now: %WC_FREE%)
goto :EOF

:: MB value -> "512 MB" / "35.37 GB"
:fmt_mb
if "%~1"=="" (set "WC_TXT=?" & goto :EOF)
set "WC_TXT=%~1 MB"
if %~1 LSS 1024 goto :EOF
set /a "_g=%~1/1024"
set /a "_m=(%~1-_g*1024)*100/1024"
if %_m% LSS 10 set "_m=0%_m%"
set "WC_TXT=%_g%.%_m% GB"
goto :EOF

:: size + file count of a folder, as a powershell expression in %1
:size_line
set "WC_SZ=n/a"
set "WC_MB="
set "WC_N="
for /f "tokens=1,2 delims=|" %%A in ('powershell -NoProfile -Command "Get-ChildItem -Path (%~1) -Force -ErrorAction SilentlyContinue | ForEach-Object { if ($_.PSIsContainer) { Get-ChildItem -LiteralPath $_.FullName -Force -Recurse -ErrorAction SilentlyContinue } else { $_ } } | Measure-Object -Property Length -Sum | ForEach-Object { $s=[int64]([math]::Round($_.Sum/1MB)); '{0}|{1}' -f $s,$_.Count }" 2^>nul') do (set "WC_MB=%%A" & set "WC_N=%%B")
if not defined WC_MB goto :EOF
call :fmt_mb %WC_MB%
set "WC_SZ=%WC_TXT%  (%WC_N% files)"
goto :EOF

:: one aligned analyse row: size, file count, label
:row
call :size_line "%~2"
set "WC_A=%WC_MB%"
set "WC_B=%WC_N%"
if not defined WC_A set "WC_A=?"
if not defined WC_B set "WC_B=?"
set "WC_A=        %WC_A%"
set "WC_B=       %WC_B%"
echo   %WC_A:~-8% MB  %WC_B:~-7% files  %~1
goto :EOF

:clean_dir
if "%~1"=="" goto :EOF
if not exist "%~1\" goto :EOF
del /s /f /q "%~1\*" >nul 2>&1
for /d %%D in ("%~1\*") do if exist "%%D\" rd /s /q "%%D" 2>nul
goto :EOF

:: browser profile caches, rebuilt by the browser itself
:clean_cache
for /d %%C in ("%~1\*Cache") do call :clean_dir "%%~fC"
goto :EOF

:clean_ff_cache
call :clean_dir "%~1\cache2"
call :clean_dir "%~1\startupCache"
goto :EOF

:: is a browser running right now? sets %busy%
:browsers_running
set "busy="
tasklist /fi "imagename eq chrome.exe" 2>nul | findstr /i /c:"chrome.exe" >nul && set "busy=%busy% chrome"
tasklist /fi "imagename eq msedge.exe" 2>nul | findstr /i /c:"msedge.exe" >nul && set "busy=%busy% edge"
tasklist /fi "imagename eq firefox.exe" 2>nul | findstr /i /c:"firefox.exe" >nul && set "busy=%busy% firefox"
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
echo   - clearing the delivery optimization cache...
powershell -NoProfile -Command "Delete-DeliveryOptimizationCache -Force" >nul 2>&1
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
echo   - preparing disk cleanup profiles (safe categories only)...
set "cm_total=0"
set "cm_kept=0"
for /f "delims=" %%K in ('reg query "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches" 2^>nul ^| findstr /b /c:"HKEY_"') do call :cm_flag "%%~nxK" "%%K"
echo   - %cm_total% categories prepared, %cm_kept% protected and left alone.
echo   - running Disk Cleanup, a window will appear...
cleanmgr /sagerun:1
echo   done.
goto :EOF

:: enable one disk cleanup category - unless it holds user data or a rollback
:cm_flag
set /a cm_total+=1
echo(%~1| findstr /x /i /c:"Recycle Bin" /c:"Previous Installations" /c:"Windows ESD installation files" /c:"User file versions" /c:"Downloads" >nul
if errorlevel 1 (
    reg add "%~2" /v StateFlags0001 /t REG_DWORD /d 2 /f >nul 2>&1
    goto :EOF
)
set /a cm_kept+=1
reg delete "%~2" /v StateFlags0001 /f >nul 2>&1
echo       kept: %~1
goto :EOF

:step_repair
echo   - step 1/2: DISM RestoreHealth (can take 5-10 minutes)
Dism.exe /online /Cleanup-Image /RestoreHealth
set "RC=%errorlevel%"
if not "%RC%"=="0" (
    echo   [!] DISM failed with error %RC%.
    echo       SFC needs a healthy component store, so it is skipped now.
    echo       Retry later, preferably with a working internet connection.
    exit /b 1
)
echo   - step 2/2: System File Checker
sfc /scannow
if errorlevel 1 (
    echo   [!] SFC found files it could not repair - see the window above.
    exit /b 1
)
echo   done. both checks finished without errors.
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

:opt_browser
call :head "BROWSER CACHE"
call :log "browser cache started"
echo   Only the cache is deleted. Passwords, cookies, history and
echo   bookmarks stay where they are - the browser rebuilds the cache
echo   on its next start.
echo.
call :browsers_running
if not defined busy goto :browser_go
echo   [!] still running:%busy%
echo       close them first: files in use cannot be deleted anyway.
echo.
choice /c YN /m "  clean anyway? (y/n) "
if errorlevel 2 goto :menu
:browser_go
echo   - chrome...
for /d %%P in ("%LOCALAPPDATA%\Google\Chrome\User Data\*") do call :clean_cache "%%~fP"
echo   - edge...
for /d %%P in ("%LOCALAPPDATA%\Microsoft\Edge\User Data\*") do call :clean_cache "%%~fP"
echo   - firefox...
for /d %%P in ("%LOCALAPPDATA%\Mozilla\Firefox\Profiles\*") do call :clean_ff_cache "%%~fP"
echo   done.
call :log "browser cache finished"
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

:opt_recycle
call :head "RECYCLE BIN"
call :log "recycle bin opened"
call :size_line "$env:SystemDrive+'\$Recycle.Bin'"
echo   currently in the recycle bin: %WC_SZ%
echo.
echo   Emptying it frees the space for good - there is no undo.
echo.
choice /c YN /m "  empty the recycle bin now? (y/n) "
if errorlevel 2 goto :menu
echo.
powershell -NoProfile -Command "Clear-RecycleBin -Force -ErrorAction SilentlyContinue" >nul 2>&1
if errorlevel 1 (
    echo   [!] it could not be emptied - the bin may already be empty.
) else (
    echo   [ok] the recycle bin is empty now.
)
call :log "recycle bin emptied"
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

:opt_analyze
call :head "ANALYZE DISK"
call :log "analyze started"
echo   Nothing is deleted here - this only measures. Numbers are
echo   approximate, other programs keep writing while it runs.
echo.
echo   -------- MB  ------- files  what can be cleaned
echo   -----------  -------------  ------------------------------
call :row "user temp" "$env:TEMP"
call :row "system temp" "$env:WINDIR+'\Temp'"
call :row "public temp" "$env:PUBLIC+'\temp'"
call :row "windows update downloads" "$env:WINDIR+'\SoftwareDistribution\Download'"
call :row "delivery optimization" "$env:WINDIR+'\SoftwareDistribution\DeliveryOptimization'"
call :row "recycle bin" "$env:SystemDrive+'\$Recycle.Bin'"
call :row "error report archive" "$env:ProgramData+'\Microsoft\Windows\WER\ReportArchive'"
call :row "chrome cache" "$env:LOCALAPPDATA+'\Google\Chrome\User Data\*\*Cache'"
call :row "edge cache" "$env:LOCALAPPDATA+'\Microsoft\Edge\User Data\*\*Cache'"
call :row "firefox cache" "$env:LOCALAPPDATA+'\Mozilla\Firefox\Profiles\*\cache2'"
echo.
echo   free space on %SystemDrive% %WC_FREE%
call :log "analyze finished"
goto :finish

:opt_driver
call :head "DRIVER CHECK / UPDATE"
call :log "driver check opened"
set "drvcount=0"
for /f %%L in ('pnputil /enum-drivers 2^>nul ^| findstr /r /c:"oem[0-9][0-9]*\.inf"') do set /a drvcount+=1
echo   %drvcount% driver packages are in the driver store.
echo.
echo   Drivers are updated through Windows Update or the pc maker's tool.
echo.
echo    1  Open Windows Update     (recommended)
echo    2  List installed drivers  (pnputil)
echo    3  Back up drivers         copy them to a folder
echo    0  Back
echo.
choice /c 1230 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="4" goto :menu
if "%SUBSEL%"=="3" goto :driver_export
if "%SUBSEL%"=="2" goto :driver_list
echo.
echo   opening Windows Update...
start "" ms-settings:windowsupdate
goto :finish

:driver_list
echo.
pnputil /enum-drivers | more
goto :finish

:driver_export
set "drvout=%USERPROFILE%\Desktop\winclean-drivers"
if not exist "%USERPROFILE%\Desktop" set "drvout=%USERPROFILE%\Documents\winclean-drivers"
if not exist "%drvout%" set "drvout=%TEMP%\winclean-drivers"
if not exist "%drvout%" mkdir "%drvout%" 2>nul
echo.
echo   exporting the drivers to:
echo     %drvout%
echo.
pnputil /export-driver * "%drvout%"
echo.
echo   done - keep that folder, it is your rollback if a driver breaks.
call :log "drivers exported"
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
if "%SUBSEL%"=="3" goto :menu
if "%SUBSEL%"=="2" goto :locker_unlock
goto :locker_lock

:locker_lock
echo.
set "target="
set /p "target=  folder to lock (full path): "
set "target=%target:"=%"
if not defined target goto :menu
if not exist "%target%\" (
    echo   [!] not a folder: %target%
    goto :menu
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
    goto :menu
)
set "pw2="
set /p "pw2=  repeat password: "
set "pw2=%pw2:"=%"
if not "%pw%"=="%pw2%" (
    echo   [!] the passwords do not match.
    goto :menu
)
> "%LOCK_FILE%" set /p "=%pw%" <nul

echo   locking %target% ...
attrib +h "%target%" >nul 2>&1
icacls "%target%" /deny "*S-1-1-0:(OI)(CI)(F)" >nul 2>&1
if errorlevel 1 (
    attrib -h "%target%" >nul 2>&1
    echo   [!] icacls failed, the folder was not locked.
    goto :menu
)
call :log "folder locked"
echo   [ok] locked. use menu A to unlock it with this password.
goto :finish

:locker_unlock
echo.
set "target="
set /p "target=  folder to unlock (full path): "
set "target=%target:"=%"
if not defined target goto :menu
if not exist "%target%\" (
    echo   [!] not a folder: %target%
    goto :menu
)
set "pw="
set /p "pw=  password: "
set "pw=%pw:"=%"
set "stored="
if exist "%LOCK_FILE%" set /p stored=<"%LOCK_FILE%"
if not defined stored (
    echo   [!] no password saved yet, lock a folder first.
    goto :menu
)
if not "%pw%"=="%stored%" (
    echo   [!] wrong password.
    call :log "folder unlock failed"
    goto :menu
)
icacls "%target%" /remove:d *S-1-1-0 >nul 2>&1
attrib -h "%target%" >nul 2>&1
call :log "folder unlocked"
echo   [ok] unlocked.
goto :finish

:locker_guard
echo   [!] this folder is protected, pick another one.
goto :menu

:opt_disk
call :head "DISK HEALTH"
call :log "disk check opened"
set "target="
set /p "target=  drive letter (example C): "
set "target=%target:"=%"
if not defined target goto :menu
set "drive=%target:~0,1%:"
if not exist "%drive%\" (
    echo   [!] drive not found: %drive%
    goto :menu
)
echo.
echo    1  Check only      read-only, only reports problems
echo    2  Check and fix   chkdsk /F, may ask for a restart
echo    0  Back
echo.
choice /c 120 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="3" goto :menu
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
    goto :menu
)
echo   Windows restarts and tests the memory. Save your work first.
echo.
start "" "%SystemRoot%\System32\mdsched.exe"
echo   [ok] choose "Restart now" or "Schedule a restart" in the window.
goto :finish

:opt_net
call :head "NETWORK FIX"
call :log "network menu opened"
echo   Troubleshooting, not cleanup. Use these when a page does not
echo   load, or a program lost its network connection.
echo.
echo    1  Flush DNS cache      ipconfig /flushdns, no restart needed
echo    2  Reset Winsock        netsh winsock reset, needs a restart
echo    3  Both
echo    0  Back
echo.
choice /c 1230 /n /m "  select > "
set "SUBSEL=%errorlevel%"
if "%SUBSEL%"=="4" goto :menu
if "%SUBSEL%"=="3" goto :net_both
if "%SUBSEL%"=="2" goto :net_winsock
goto :net_dns

:net_dns
echo.
echo   - flushing the DNS resolver cache...
ipconfig /flushdns
call :log "dns cache flushed"
goto :finish

:net_winsock
echo.
echo   [!] this resets the Winsock catalog. Software that installs a
echo       network filter (VPN, antivirus, proxy tools) may have to be
echo       reinstalled, and a restart finishes the reset.
echo.
choice /c YN /m "  continue? (y/n) "
if errorlevel 2 goto :menu
echo.
netsh winsock reset
call :log "winsock reset"
goto :finish

:net_both
call :net_dns_winsock
goto :finish

:net_dns_winsock
echo.
echo   - flushing the DNS resolver cache...
ipconfig /flushdns
echo   [!] this also resets the Winsock catalog - a restart finishes it.
echo.
choice /c YN /m "  continue? (y/n) "
if errorlevel 2 goto :menu
echo.
netsh winsock reset
call :log "dns flushed and winsock reset"
goto :EOF

:opt_restore
call :head "RESTORE POINT"
call :log "restore point requested"
echo   A restore point snapshots system files and settings, so you can
echo   roll a change back. It is not a backup of your documents.
echo.
echo   creating it, this can take a minute...
powershell -NoProfile -Command "try { Checkpoint-Computer -Description 'winclean %ver%' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop; exit 0 } catch { exit 1 }"
if errorlevel 1 (
    echo   [!] it did not work. The usual reasons are:
    echo       - system protection is switched off for this drive
    echo       - a restore point was already made in the last 24 hours
) else (
    echo   [ok] restore point created.
)
goto :finish

:opt_sysinfo
call :head "SYSTEM INFO"
call :log "system info opened"
set "cvkey=HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion"
set "cvname=unknown"
set "cvver="
set "cvbuild="
set "cvubr=0"
set "cvedition="
set "cpuname=unknown"
set "ramgb=?"
set "dsktot=0"
for /f "tokens=2,*" %%A in ('reg query "%cvkey%" /v ProductName 2^>nul ^| findstr /i "REG_SZ"') do set "cvname=%%B"
for /f "tokens=2,*" %%A in ('reg query "%cvkey%" /v DisplayVersion 2^>nul ^| findstr /i "REG_SZ"') do set "cvver=%%B"
for /f "tokens=2,*" %%A in ('reg query "%cvkey%" /v CurrentBuildNumber 2^>nul ^| findstr /i "REG_SZ"') do set "cvbuild=%%B"
for /f "tokens=2,*" %%A in ('reg query "%cvkey%" /v UBR 2^>nul ^| findstr /i "REG_DWORD"') do set "cvubr=%%B"
set /a "cvubrn=%cvubr%"
for /f "tokens=2,*" %%A in ('reg query "%cvkey%" /v EditionID 2^>nul ^| findstr /i "REG_SZ"') do set "cvedition=%%B"
for /f "delims=" %%B in ('powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).Name" 2^>nul') do set "cpuname=%%B"
for /f "delims=" %%B in ('powershell -NoProfile -Command "(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory/1GB -as [int]" 2^>nul') do set "ramgb=%%B"
for /f "delims=" %%B in ('powershell -NoProfile -Command "((Get-PSDrive %SystemDrive:~0,1%).Used+(Get-PSDrive %SystemDrive:~0,1%).Free)/1MB -as [int]" 2^>nul') do set "dsktot=%%B"
if not defined dsktot set "dsktot=0"
if not defined cvver set "cvver=unknown"
if not defined cvbuild set "cvbuild=?"
if not defined cvedition set "cvedition=unknown"
call :fmt_mb %dsktot%
set "dsktot_txt=%WC_TXT%"
echo   windows      %cvname%
echo   version      %cvver%   (build %cvbuild%.%cvubrn%)
echo   edition      %cvedition%
echo   cpu          %cpuname%
echo   ram          %ramgb% GB
echo   disk %SystemDrive%       %WC_FREE% free of %dsktot_txt%
echo.
echo   winclean     %WC_DIR%
echo   log file     %MAIN_LOG%
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
goto :do_exit

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
if "%SUBSEL%"=="16" goto :menu
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
if not defined newuser goto :menu
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
if "%SUBSEL%"=="3" goto :menu
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
    goto :menu
)
more < "%MAIN_LOG%"
goto :finish
