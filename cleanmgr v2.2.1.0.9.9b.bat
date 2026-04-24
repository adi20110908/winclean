@echo off
title winclean booting...
cls
:: ##release: 2026.01.31
:: TUDJA AZ ÉKEZETEKET!!
:: version slogen: folderlocker -- betaversion -- 1% //bootupdate
color 3
:: admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    color C1
    echo NEEDS ADMIN RIGHTS!
    echo failed loading winclean
    echo restarting with admin rights...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

	cls
echo Loading...
	echo [=       ]

													set ver=v2.2.1.0.9.9b
				title winclean loading %ver%
	cls
echo Loading...
	echo [==      ]
				color B1
				chcp 65001 >nul
				c:

	cls
echo Loading...
	echo [===     ]
				timeout /t 1 >nul
				cd..
				cd..

	cls
echo Loading...
	echo [====    ]
				cd Windows
				cd System32


	cls
echo Loading...
	echo [=====   ]
				set color=B
	cls
echo Loading...
	echo [======  ]
				set colorprofile= Text color: Light Aqua (default)
	cls
echo Loading...
	echo [======= ]
				timeout /t 1 >nul
				title winclean %ver%
	cls
echo Loading...
	echo [========]
cls

:: ============================== BOOOT END ============================ ::

:MENU
cls
color %color%
title winclean %ver%
echo ============================= WELCOME TO WINCLEAN! =============================
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
echo Option 7:  Change MENU color (until the app is opened) --betaversion
echo Option 8:  -- not working -- Folder locker -- beta
echo Option 9:  MODE: Repair, Delete 
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

) else if "%option%"=="01" (
echo heading to restart
cls
goto restart

) else (
    echo unknown option
    echo press any key to choose another one
    timeout /t 5 >nul
    goto menu
)


:: ====================================================== CHOOSER END ====================================================== ::

:colorch
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
goto colorch

) else if "%coloruser%"=="2" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=02
set colorprofile= Text color: Green
goto colorch

) else if "%coloruser%"=="3" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=03
set colorprofile= Text color: Aqua
goto colorch

) else if "%coloruser%"=="4" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=04
set colorprofile= Text color: Red
goto colorch

) else if "%coloruser%"=="5" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=05
set colorprofile= Text color: Purple
goto colorch

) else if "%coloruser%"=="6" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=06
set colorprofile= Text color: Yellow
goto colorch

) else if "%coloruser%"=="7" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=07
set colorprofile= Text color: White
goto colorch

) else if "%coloruser%"=="8" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=08
set colorprofile= Text color: Gray
goto colorch

) else if "%coloruser%"=="9" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=09
set colorprofile= Text color: Light Blue
goto colorch

) else if "%coloruser%"=="10" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=A
set colorprofile= Text color: Light Green
goto colorch

) else if "%coloruser%"=="11" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=B
set colorprofile= Text color: Light Aqua
goto colorch

) else if "%coloruser%"=="12" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=C
set colorprofile= Text color: Light Red
goto colorch

) else if "%coloruser%"=="13" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=D
set colorprofile= Text color: Light Purple
goto colorch

) else if "%coloruser%"=="14" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=E
set colorprofile= Text color: Light Yellow
goto colorch

) else if "%coloruser%"=="15" (
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=F
set colorprofile= Text color: Bright White
goto colorch

) else if "%coloruser%"=="16" (
echo I SAID COMMING SOON
pause
goto colorch
::
echo changeing color to %coloruser%
timeout /t 1 >nul
set color=F
set colorprofile= Text color: Bright White
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


:: ============================== WINGET UPDATE ====================== ::
goto MENU
:update
echo winget update in process...
winget update --all

timeout /t 3 >nul
echo successfully updated!
timeout /t 1 >nul /nobreak
goto rate

:: ============================= UPDATER END ============================= ::

:driver
cls
echo driver update in progress...
driverquery /v
timeout /t 1 /nobreak >nul
echo just a second...

driverquery /si
goto rate
:: ============================== DRIVER END =========================== ::

:temp
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
goto rate
:: =========================== TEMPDEL END ======================= ::

:clean
cls
echo tempomary files deleting in progress...
cleanmgr /sagerun:1
timeout /t 3 >nul /nobreak
echo SUCCESS!
timeout /t 1 >nul /nobreak
goto rate
:: ========================= CLEAN END ============================ ::

:corrupt
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
goto rate
:: ========================= CORRUPT END ============================ ::

:: ======================================= REPAIR / DELETE ======================================= ::
:repair
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
goto rate
:: ======================================= REPAIR / DELETE END ======================================= ::

:: ====================== FULLVER ============================= :: <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
:fullver

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
goto exit
:: ============================= FULLVER END ============================ ::

:dirlock

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


:: ========================= RATE ============================== ::
:rate
cls
echo warning panel are showing up! please wait!
echo Set s = CreateObject("WScript.Shell") > wincleanpopup.vbs
echo s.Popup "operation complete", 6, "winclean %ver% popup window", 64 >> wincleanpopup.vbs
start /wait wincleanpopup.vbs
del wincleanpopup.vbs

cls
echo RATE!
set /p rate= Rate the winclean app! (write numbers between 1-10): 
timeout /t 1 >nul
cls
echo Thank you! You rated to: %rate%
echo.
echo press any button to return to menu!
pause >nul
goto menu


:exit
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
exit

:restart
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

call "%~f0"
exit /b