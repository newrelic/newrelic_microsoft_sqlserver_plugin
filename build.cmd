@echo Off
set target=%1
if "%target%" == "" (
   set target=Go
)
set config=%2
if "%config%" == "" (
   set config=Debug
)

:: artifact dir
if not exist local\ (
	mkdir local
)

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild build\build.proj /t:"%target%" /p:Configuration="%config%" /m /fl /flp:LogFile=local\msbuild.log;Verbosity=Normal /nr:false