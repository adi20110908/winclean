@echo off
title winclean booting...
cls
:: ##release: 2026.03.22
:: TUDJA AZ ÉKEZETEKET!!
:: version slogen: folderlocker -- betaversion -- 1% //bootupdate //loggerupdate //username //ramscan




	cls
				color 3
echo Loading...
	echo [=       ]

				:: admin rights
				net session >nul 2>&1
				if %errorlevel% neq 0 (
				    cls
				    color C1
				    echo failed loading winclean: ERROR: nopermission "(NEEDS ADMIN RIGHTS)"
				    echo restarting with admin rights...
				    powershell -Command "Start-Process '%~f0' -Verb RunAs"
				   exit /b
				)

				chcp 65001 >nul

	cls
echo Loading...
	echo [==      ]

set "WC_DIR=%APPDATA%\winclean"
set "LOG_DIR=%WC_DIR%\logs"

if not exist "%WC_DIR%" (
    mkdir "%WC_DIR%"
)

if not exist "%LOG_DIR%" (
    mkdir "%LOG_DIR%"
)


:: ============================= FILES / NAMES ============================== :: 
set "MAIN_LOG=%LOG_DIR%\winclean.log"
set "LATEST_LOG=%~dp0latest.log"

if exist "%LATEST_LOG%" del "%LATEST_LOG%" >nul 2>&1

if not exist "%MAIN_LOG%" (
    echo ============================= > "%MAIN_LOG%"
    echo WINCLEAN LOG >> "%MAIN_LOG%"
    echo Created: %DATE% %TIME% >> "%MAIN_LOG%"
    echo ============================= >> "%MAIN_LOG%"
)

(
echo =============================
echo WINCLEAN LATEST SESSION LOG
echo Started: %DATE% %TIME%
echo =============================
) > "%LATEST_LOG%"



	cls
echo Loading...
	echo [===     ]



														set ver=v3.7.8
		title winclean loading %ver%
		set color=B
		color B1

	cls
echo Loading...
	echo [====    ]

set WC_DIR=%APPDATA%\winclean
set COLOR_FILE=%WC_DIR%\colormode.cfg

if not exist "%WC_DIR%" (
    mkdir "%WC_DIR%"
)

if exist "%COLOR_FILE%" (
    set /p color=<"%COLOR_FILE%"
    call :LOG "color loaded from AppData: %color%"
) else (
    set color=B
    echo %color%>"%COLOR_FILE%"
    call :LOG "COLOR created default: %color%"
)


	cls
echo Loading...
	echo [=====   ]

set WC_DIR=%APPDATA%\winclean
set COLORPROFILE_FILE=%WC_DIR%\colorprofile.cfg

if not exist "%WC_DIR%" (
    mkdir "%WC_DIR%"
)

if exist "%COLORPROFILE_FILE%" (
    set /p colorprofile=<"%COLORPROFILE_FILE%"
) else (
    set colorprofile= Text color: Light Aqua
)



	cls
echo Loading...
	echo [======  ]

				cd..
				cd..
				cd..
				cd..
				cd..
				cd..
				c:\
				cd..
				cd..
				cd Windows
				cd System32


set WC_DIR=%APPDATA%\winclean
set USER_FILE=%WC_DIR%\user.cfg

if not exist "%WC_DIR%" (
    mkdir "%WC_DIR%"
)

if exist "%USER_FILE%" (
    set /p user=<"%USER_FILE%"
    call :LOG "Username loaded from AppData: %user%"
) else (
    set user=User
    echo %user%>"%USER_FILE%"
    call :LOG "Username created default: %user%"
)


	cls
echo Loading...
	echo [======= ]


				title winclean %ver%

	cls
echo Loading...
	echo [========]
cls
call :LOG "boot ended"
call :LOG "winclean %ver% running"
call :LOG_DEVICE


:: ============================== BOOOT END ============================ ::

:MENU
cls
call :LOG "MENU opened"
color %color%
title winclean %ver%
echo ============================= WELCOME %user% TO WINCLEAN! =============================
echo.
echo winclean ver: %ver%
echo.
echo.
echo Choose an option from the list:
echo.
echo Option 1:  Driver update / check
echo Option 2:  Temp delete
echo Option 3:  Tempomary files delete (explain: old windows update)
echo Option 4:  Corrupted files repair
echo Option 5:  Winget update
echo Option 6:  MODE: RUN ALL and autoexit
echo Option 7:  Change MENU color
echo Option 8:  -- not working -- Folder locker -- beta
echo Option 9:  MODE: Repair, Delete 
echo Option 10: Usernamechanger
echo Option 11: Ram checker
echo.
echo Option 0:  exit
echo Option 01: Restarts winclean %ver%


echo.
set /p option=Option: 
if "%option%"=="0" (
  echo heading to quit
  cls
  goto exit

) else if "%option%"=="1" (
echo heading to driver
goto driver

) else if "%option%"=="2" (
echo heading to temp
cls
goto temp

) else if "%option%"=="3" (
echo heading to clean
cls
goto clean

) else if "%option%"=="4" (
echo heading to corrupt
cls
goto corrupt

) else if "%option%"=="5" (
echo heading to winget update
cls
goto update

) else if "%option%"=="6" (
echo heading to driver fullversion mode
cls
goto fullver

) else if "%option%"=="7" (
echo heading to chcolor
cls
goto colorch

) else if "%option%"=="8" (
echo heading to Folder locker
cls
goto dirlock

) else if "%option%"=="9" (
echo heading to Repair delete
cls
goto repair

) else if "%option%"=="10" (
echo heading to usernamechanger
cls
goto usernamech

) else if "%option%"=="11" (
echo heading to ramscan
cls
goto ramscan

) else if "%option%"=="01" (
echo heading to restart
cls
goto restart

) else if "%option%"=="todo" (
echo heading totototototodooo
cls
goto todo

) else (
    echo unknown option
    echo press any key to choose another one
    timeout /t 5 >nul
    goto menu
)


:: ====================================================== CHOOSER END ====================================================== ::

:colorch
call :LOG "colorch opened"
cls
color %color%
echo =========================== COLOR CHANGER ===========================
echo.
echo Now you are using this colorprofile: %colorprofile%
echo.
echo Aviable colors:
echo 1:  Text color: Blue
echo 2:  Text color: Green
echo 3:  Text color: Aqua
echo 4:  Text color: Red
echo 5:  Text color: Purple
echo 6:  Text color: Yellow
echo 7:  Text color: White
echo 8:  Text color: Gray
echo 9:  Text color: Light Blue
echo 10: Text color: Light Green
echo 11: Text color: Light Aqua
echo 12: Text color: Light Red
echo 13: Text color: Light Purple
echo 14: Text color: Light Yellow
echo 15: Text color: Bright White
echo 16: comming soon...

echo.
echo 0: go back to menu
echo.

set /p coloruser=Color: 
if "%coloruser%"=="0" (
  echo heading to menu
  cls
  goto menu

) else if "%coloruser%"=="1" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=01
set colorprofile= Text color: Blue
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="2" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=02
set colorprofile= Text color: Green
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="3" (
    echo changeing color to %coloruser%
    timeout /t 1 >nul
    set color=03
    set colorprofile= Text color: Aqua
    call :SAVE_COLOR
    goto colorch


) else if "%coloruser%"=="4" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=04
set colorprofile= Text color: Red
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="5" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=05
set colorprofile= Text color: Purple
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="6" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=06
set colorprofile= Text color: Yellow
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="7" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=07
set colorprofile= Text color: White
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="8" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=08
set colorprofile= Text color: Gray
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="9" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=09
set colorprofile= Text color: Light Blue
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="10" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=A
set colorprofile= Text color: Light Green
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="11" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=B
set colorprofile= Text color: Light Aqua
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="12" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=C
set colorprofile= Text color: Light Red
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="13" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=D
set colorprofile= Text color: Light Purple
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="14" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=E
set colorprofile= Text color: Light Yellow
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="15" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=F
set colorprofile= Text color: Bright White
    call :SAVE_COLOR
goto colorch

) else if "%coloruser%"=="16" (
echo I SAID COMMING SOON
pause
goto colorch

) else (
    echo unknown option
    color C4
    timeout /t 1 >nul
    echo press any key to choose another one
    Color C1
    pause >nul
    goto colorch
)


:: ================================================ COLORCH END ============================================== ::

:: ======================= USERCH =========================== ::
:usernamech
set /p username=Username:  
set user=%username%
echo %user%>"%USER_FILE%"
call :LOG "username changed and saved: %user%"
echo username set to %user%
echo.
echo press any key to return to menu
pause >nul
goto MENU

:: ===================== USERCH END ========================= ::

:: ============================== WINGET UPDATE ====================== ::
goto MENU
:update
call :LOG "winget update in progress..."
echo winget update in process...
winget update --all

timeout /t 3 >nul
echo successfully updated!
call :LOG "winget update finished!"
timeout /t 1 >nul /nobreak
goto rate

:: ============================= UPDATER END ============================= ::

:driver
call :LOG "drivers update in progress..."
cls
echo driver update in progress...
driverquery /v
timeout /t 1 /nobreak >nul
echo just a second...

driverquery /si
call :LOG "driver update finished!"
goto rate
:: ============================== DRIVER END =========================== ::

:temp
call :LOG "temp delete in progress..."

cls
echo Temps deleteing in progess. please wait...
timeout /t 1 /nobreak >nul

echo User temp deleteing...
del /s /f /q "%TEMP%\*.*"
for /d %%x in ("%TEMP%\*") do rd /s /q "%%x"
echo SUCCESS

echo System temp deleteing
del /s /f /q "C:\Windows\Temp\*.*"
for /d %%x in ("C:\Windows\Temp\*") do rd /s /q "%%x"
echo SUCCESS


echo Public users temp deleteing
del /s /f /q "C:\Users\Public\temp*.*"
for /d %%x in ("C:\Users\Public\temp*") do rd /s /q "%%x"
echo SUCCESS

timeout /t 1 >nul /nobreak
call :LOG "temp delete finished"

goto rate
:: =========================== TEMPDEL END ======================= ::

:clean
call :LOG "tempomary files deleting in progress..."
cls
echo tempomary files deleting in progress...
cleanmgr /sagerun:1
timeout /t 3 >nul /nobreak
echo SUCCESS!
timeout /t 1 >nul /nobreak
call :LOG "tempomary files delete finished!"
goto rate
:: ========================= CLEAN END ============================ ::

:corrupt
call :LOG "corrupted files repearing..."
cls
echo corrupted files repearing...
Dism.exe /online /Cleanup-Image /StartComponentCleanup
timeout /t 3 >nul /nobreak
echo SUCCESS!

cls
echo corrupted files repearing... (sfc)
sfc /scannow
timeout /t 3 >nul /nobreak
echo SUCCESS!
call :LOG "corrupted files repeared!"
goto rate
:: ========================= CORRUPT END ============================ ::

:: ======================================= REPAIR / DELETE ======================================= ::
:repair
call :LOG "Repair started..."
cls
echo Temps deleteing in progess. please wait...
timeout /t 1 /nobreak >nul

echo User temp deleteing...
del /s /f /q "%TEMP%\*.*"
for /d %%x in ("%TEMP%\*") do rd /s /q "%%x"
echo SUCCESS

echo System temp deleteing
del /s /f /q "C:\Windows\Temp\*.*"
for /d %%x in ("C:\Windows\Temp\*") do rd /s /q "%%x"
echo SUCCESS


echo Public users temp deleteing
del /s /f /q "C:\Users\Public\temp*.*"
for /d %%x in ("C:\Users\Public\temp*") do rd /s /q "%%x"
echo SUCCESS

timeout /t 1 >nul /nobreak

cls
echo tempomary files deleting in progress...
cleanmgr /sagerun:1
timeout /t 3 >nul /nobreak
echo SUCCESS!
timeout /t 1 >nul /nobreak

cls
echo corrupted files repearing...
Dism.exe /online /Cleanup-Image /StartComponentCleanup
timeout /t 3 >nul /nobreak
echo SUCCESS!

cls
echo corrupted files repearing...
sfc /scannow
timeout /t 3 >nul /nobreak
echo SUCCESS!
call :LOG "Repair finished"
goto rate
:: ======================================= REPAIR / DELETE END ======================================= ::

:: ======================================= RAMSCAN ======================================= ::
:ramscan
echo opening the ram checker user menu
C:\Windows\Sysnative\mdsched.exe
echo menu opened. You will needed to select an option from the opened list.
timeout /t 5
echo the tab is now closeing..
timeout /t 2
goto rate


:: ======================================= RAMSCAN END ======================================= ::

:: ====================== FULLVER ============================= :: <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
:fullver
call :LOG "FULLVER started..."

echo winget update in process...
winget update --all
timeout /t 3 >nul
echo successfully updated!
timeout /t 1 >nul /nobreak

:driver
cls
echo driver update in progress...
driverquery /v
timeout /t 1 /nobreak >nul
echo just a second...
driverquery /si

cls
echo Temps deleteing in progess. please wait...
timeout /t 1 /nobreak >nul

echo User temp deleteing...
del /s /f /q "%TEMP%\*.*"
for /d %%x in ("%TEMP%\*") do rd /s /q "%%x"
echo SUCCESS

echo System temp deleteing
del /s /f /q "C:\Windows\Temp\*.*"
for /d %%x in ("C:\Windows\Temp\*") do rd /s /q "%%x"
echo SUCCESS


echo Public users temp deleteing
del /s /f /q "C:\Users\Public\temp*.*"
for /d %%x in ("C:\Users\Public\temp*") do rd /s /q "%%x"
echo SUCCESS

timeout /t 1 >nul /nobreak

cls
echo tempomary files deleting in progress...
cleanmgr /sagerun:1
timeout /t 3 >nul /nobreak
echo SUCCESS!
timeout /t 1 >nul /nobreak

cls
echo corrupted files repearing...
Dism.exe /online /Cleanup-Image /StartComponentCleanup
timeout /t 3 >nul /nobreak
echo SUCCESS!

cls
echo corrupted files repearing...
sfc /scannow
timeout /t 3 >nul /nobreak
echo SUCCESS!
cls
echo fullupdate success! heading to exit...
timeout /t 3 >nul
call :LOG "FULLVER finished"
goto exit
:: ============================= FULLVER END ============================ ::

:dirlock
echo this is not aviable now (maybe in future)
pause 
goto exit

cacls "mappa" /e /p everyone:n

cacls "mappa" /e /p everyone:f


pause
pause
pause
pause
pause
pause
pause
pause
pause

:: =================== DIRLOCK END ========================= ::

:: ========================= RATE ============================== ::
:rate
call :LOG "Rate opened"

cls
echo warning panel are showing up! please wait!
echo Set s = CreateObject("WScript.Shell") > wincleanpopup.vbs
echo s.Popup "operation complete", 6, "winclean %ver% popup window", 64 >> wincleanpopup.vbs
start /wait wincleanpopup.vbs
del wincleanpopup.vbs

cls
echo RATE!
set /p rate= Rate the winclean app! (write numbers between 1-10): 
call :LOG "%user% rated to %rate%"
timeout /t 1 >nul
cls
echo Thank you! You rated to: %rate%
echo.
echo press any button to return to menu!
pause >nul
goto menu


:: ============================================= LOGGER SYSTEM ============================================= ::

:: =========== savecolor sth =========== ::
:SAVE_COLOR
echo %color%>"%COLOR_FILE%"
echo %colorprofile%>"%COLORPROFILE_FILE%"
:: =========== savecolor end =========== ::

:LOG_DEVICE
set DEVICE_NAME=%COMPUTERNAME%
call :LOG "devicename: %DEVICE_NAME%"
goto :EOF

:LOG
set MSG=%~1
set TIME_STR=%TIME:~0,8%
set LINE=[%DATE% %TIME_STR%] %MSG%
echo %LINE% >> "%MAIN_LOG%"
echo %LINE% >> "%LATEST_LOG%"
goto :EOF


:: ============================================= LOGGER END ============================================= ::

:: ============================================ QUIT =========================================== ::

:exit
call :LOG "winclean closeing..."
title winclean quiting %ver%
cls
echo Thank you for using winclean by adiservers-pro!
echo Quiting...
	echo [=====]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Quiting...
	echo [==== ]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Quiting...
	echo [===  ]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Quiting...
	echo [==   ]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Quiting...
	echo [=    ]
	timeout /t 1 >nul
call :LOG "winclean closed!"
exit

:restart
call :LOG "winclean restarting..."
echo Thank you for using winclean by adiservers-pro!
echo Restarting...
	echo [===]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Restarting...
	echo [== ]
	timeout /t 1 >nul
	cls
echo Thank you for using winclean by adiservers-pro!
echo Restarting...
	echo [=  ]
	timeout /t 1 >nul
cls
echo \
timeout /t 1 >nul /nobreak
cls
echo ^|
timeout /t 1 >nul /nobreak
cls
echo /
timeout /t 1 >nul /nobreak
cls
echo -
timeout /t 1 >nul /nobreak
cls
echo \
timeout /t 1 >nul /nobreak
cls
echo ^|
timeout /t 1 >nul /nobreak
cls
echo /
timeout /t 1 >nul /nobreak
cls
call :LOG "Winclean restarted"
call "%~f0"
exit /b


:todo
:: ================== TODO !!! ================== ::
echo " :dirlock "
echo " chkdsk C:  ---- to winclean (disk health check in 3 states) "
echo " watch log "

pause
goto MENU
